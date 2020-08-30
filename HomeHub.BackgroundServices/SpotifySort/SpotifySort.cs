using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeHub.BackgroundServices.Configuration.SpotifySort;
using HomeHub.BackgroundServices.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace HomeHub.BackgroundServices
{
    public class SpotifySort : ISpotifySort
    {
        private readonly ILogger<SpotifySort> logger;
        private readonly SpotifySortOptions options;
        private readonly IApi api;
        private PrivateProfile user;
        public bool IsAuthenticated { get; set; }
        public SpotifyAuthorizationCodeAuth Auth { get; set; }
        public string RefreshToken { get; set; }
        public Token Token { get; set; }
        private readonly IServiceScopeFactory scopeFactory;
        public SemaphoreSlim RequestSemaphore { get; }

        public SpotifySort(
            ILogger<SpotifySort> logger,
            IOptions<SpotifySortOptions> options,
            IApi api,
            IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            this.options = options.Value;
            this.api = api;
            this.scopeFactory = scopeFactory;
            RequestSemaphore = new SemaphoreSlim(this.options.MaxConcurrentThreads, this.options.MaxConcurrentThreads);
        }

        public async Task AuthenticateUserAsync(string clientId,
                                                string clientSecret,
                                                string localIp,
                                                SemaphoreSlim mainTaskSemaphore,
                                                CancellationToken cancellationToken)
        {
            await mainTaskSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Starting authentication process.");

            // If we're authenticating for the first time, we want to clear out our tokens if they exist.
            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ISpotifyContext>();
                foreach (var token in context.Tokens)
                {
                    context.Tokens.Remove(token);
                }

                context.SaveChanges();
            }

            Scope[] scopes = new Scope[]{
                Scope.UserLibraryModify,
                Scope.UserReadPrivate,
                Scope.UserReadEmail,
                Scope.UserLibraryRead,
                Scope.PlaylistModifyPublic
            };

            // Wrapper class for AuthorizationCodeAuth - Works better with inability to open a browser on pi.
            Auth = new SpotifyAuthorizationCodeAuth(
                clientId,
                clientSecret,
                $"http://{localIp}:4002",
                $"http://{localIp}:4002",
                scopes
            );

            // Delegate for when the endpoint has been accessed/approved by user.
            Auth.AuthReceived += async (sender, payload) =>
            {
                logger.LogInformation("Authentication received. Creating api object.");
                Auth.Stop();

                Token = await Auth.ExchangeCode(payload.Code);
                RefreshToken = Token.RefreshToken;

                // We got a new token, we save it.
                using (var scope = scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<ISpotifyContext>();
                    context.Tokens.Add(Token);
                    context.SaveChanges();
                }

                api.GenerateApi(Token.TokenType, Token.AccessToken);

                // Denotes that we can refresh the token in the future.
                IsAuthenticated = true;

                mainTaskSemaphore.Release();
            };

            // Starts to listen using the auth endpoint.
            Auth.Start();

            var authString = Auth.CreateUri();

            logger.LogInformation($"Please visit this link to authenticate: {authString}");
        }

        public async Task RunSortAsync(SemaphoreSlim mainTaskSemaphore, CancellationToken cancellationToken)
        {
            await mainTaskSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();

            // Stores the ID/Description association.
            Dictionary<string, string> playlistDescriptions = new Dictionary<string, string>();

            // Stores the PlaylistID/SongID relation for the multi call back to the API.
            Dictionary<string, List<string>> playlistNewSongs = new Dictionary<string, List<string>>();
            List<Task> tasks = new List<Task>();
            var playlists = GetUserPlaylistsAsync(cancellationToken);
            var likedSongs = GetUserLikedTracksAsync(cancellationToken);

            await Task.WhenAll(new Task[] { playlists, likedSongs });

            // TODO -> Move the Get Descriptions and Get Genres to their own things.
            // TODO -> This could potentially spawn 100+ requests. Consider a semaphore to help less powerful pi?

            // Creating the association between the playlist ID and description
            foreach(var playlist in playlists.Result.Items)
            {
                tasks.Add(Task.Run(async () =>
                {
                    playlistDescriptions[playlist.Id] = await GetPlaylistDescriptionsAsync(playlist.Id, cancellationToken);
                    playlistNewSongs[playlist.Id] = new List<string>();
                }));
            }

            // Making sure that the playlists are fully received before trying to sort into them.
            await Task.WhenAll(tasks);
            tasks.Clear();

            // Leveraging implicit conversion in this iteration to save on iterations elsewhere.
            foreach(SavedTrackWithGenre song in likedSongs.Result.Items)
            {
                tasks.Add(Task.Run(async () =>
                {
                    // Storing as genre track. Paging object isn't super helpful for this.
                    var songWithGenres = await GetGenreFromSongAsync(song, cancellationToken);

                    // Don't need to save the modified song anywhere IFF we can just directly add it to the dict.
                    await AddSongsToGenreDictionaryAsync(songWithGenres,
                                                         playlistDescriptions,
                                                         playlistNewSongs,
                                                         cancellationToken);
                }));
            }

            await Task.WhenAll(tasks);
            tasks.Clear();

            // Multicall for adding to playlists/removing from liked.
            await MoveNewSongsIntoPlaylistsAsync(playlistNewSongs, cancellationToken);

            mainTaskSemaphore.Release();
        }

        private async Task<Paging<SimplePlaylist>> GetUserPlaylistsAsync(CancellationToken cancellationToken)
        {
            await RequestSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();

            user ??= await GetUserAsync(cancellationToken);
            string userId = user.Id;
            logger.LogInformation($"Getting playlists from {userId}.");
            Paging<SimplePlaylist> playlists = await api.GetUserPlaylistsAsync(userId, options.MaxPlaylistResults);
            RequestSemaphore.Release();
            return playlists;
        }

        private async Task<PrivateProfile> GetUserAsync(CancellationToken cancellationToken)
        {
            await RequestSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation($"Requesting {user?.Id ?? "user"}'s Private profile.");
            PrivateProfile profile = await api.GetPrivateProfileAsync();
            RequestSemaphore.Release();
            return profile;
        }

        private async Task<Paging<SavedTrack>> GetUserLikedTracksAsync(CancellationToken cancellationToken)
        {
            await RequestSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation($"Getting {user?.Id ?? "User"}'s Liked tracks.");
            Paging<SavedTrack> tracks = await api.GetSavedTracksAsync(options.MaxSongResults);
            cancellationToken.ThrowIfCancellationRequested();
            RequestSemaphore.Release();
            return tracks;
        }

        private async Task<string> GetPlaylistDescriptionsAsync(
            string playlist,
            CancellationToken cancellationToken)
        {
            await RequestSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation($"Getting Playlist description for playlistID: {playlist}");
            FullPlaylist fullPlaylist = await api.GetPlaylistAsync(playlist);
            RequestSemaphore.Release();
            return fullPlaylist.Description;
        }

        private async Task<SavedTrackWithGenre> GetGenreFromSongAsync(
            SavedTrackWithGenre genreTrack,
            CancellationToken cancellationToken)
        {
            await RequestSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();

            foreach(var artist in genreTrack.Track.Artists)
            {
                // Could async this too if performance is really taking a hit, but it probably isn't necessary.
                logger.LogInformation($"Getting Genres for artist {artist} for song {genreTrack.Track.Name}");
                var fullArtist = await api.GetArtistAsync(artist.Id);
                genreTrack.Genres = genreTrack.Genres.Concat(fullArtist.Genres).ToList();
            }

            cancellationToken.ThrowIfCancellationRequested();
            RequestSemaphore.Release();
            return genreTrack;
        }

        private async Task AddSongsToGenreDictionaryAsync(
            SavedTrackWithGenre genreTrack,
            Dictionary<string, string> genreIdDict,
            Dictionary<string, List<string>> newSongsDict,
            CancellationToken cancellationToken)
        {
            await RequestSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation($"Adding {genreTrack.Track.Name} to genreList");

            // foreach(string genre in genreTrack.Genres)
            // {
            //     foreach(KeyValuePair<string, string> genres in genreIdDict)
            //     {
            //         // If the genre exists in any playlist genre, AND isn't already in the newSongsDict.
            //         if (genres.Value.Contains(genre) && (!newSongsDict[genres.Key].Contains(genre)))
            //         {
            //             newSongsDict[genres.Key].Add(genreTrack.Track.Id);
            //         }
            //     }
            // }

            RequestSemaphore.Release();
        }
    
        private async Task MoveNewSongsIntoPlaylistsAsync(
            Dictionary<string, List<string>> newPlaylistSongsDict,
            CancellationToken cancellationToken)
        {
            await RequestSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            List<Task> tasks = new List<Task>();
            List<string> unlikeList = new List<string>();

            foreach(KeyValuePair<string, List<string>> entry in newPlaylistSongsDict)
            {
                logger.LogInformation($"Moving tracks to playlist: {entry.Key}");
                tasks.Add(api.AddPlaylistTracksAsync(entry.Key, entry.Value));
                unlikeList = unlikeList.Concat(entry.Value).ToList();
            }

            await Task.WhenAll(tasks);
            logger.LogInformation("Unliking moved songs.");
            await api.RemoveSavedTracksAsync(unlikeList);
            RequestSemaphore.Release();
        }
    }
}