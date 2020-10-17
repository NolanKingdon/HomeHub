using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeHub.SpotifySort.Configuration;
using HomeHub.SpotifySort.Database;
using HomeHub.SpotifySort.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace HomeHub.SpotifySort
{
    /// <summary>
    /// Sorting class for the spotify service. Referenced in several other projects via extension method.
    /// Was initially going to be asynchronous, but the API Wrapper being used isn't Actually Async and
    /// relies on thread unsafe collections. In the future, I may want to write my own, thread safe wrapper.
    /// But for now, I'm going to have to use the 5+ second calls provided by the wrapper.
    /// </summary>
    public class SpotifySorter : ISpotifySort
    {
        private readonly ILogger<SpotifySorter> logger;
        private readonly SpotifySortOptions options;
        public IApi Api { get; }
        private PrivateProfile user;
        public bool IsAuthenticated { get; set; }
        public SpotifyAuthorizationCodeAuth Auth { get; set; }
        public string RefreshToken { get; set; }
        public Token Token { get; set; }
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IContextProvider contextProvider;
        public SemaphoreSlim RequestSemaphore { get; }

        public SpotifySorter(
            ILogger<SpotifySorter> logger,
            IOptions<SpotifySortOptions> options,
            IApi api,
            IServiceScopeFactory scopeFactory,
            IContextProvider contextProvider)
        {
            this.logger = logger;
            this.options = options.Value;
            Api = api;
            this.scopeFactory = scopeFactory;
            this.contextProvider = contextProvider;
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
            using (var context = contextProvider.GenerateContext<ISpotifyContext>(scopeFactory))
            {
                foreach (var token in context.Tokens)
                {
                    context.Tokens.Remove(token);
                }

                await context.SaveChangesAsync(cancellationToken);
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

            logger.LogInformation($"id - {clientId}\n secret - {clientSecret}\n localIp - {localIp}\n scopes - {scopes}");

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
                    context.Tokens.Add((SpotifyToken)Token);
                    await context.SaveChangesAsync(cancellationToken);
                }

                // Denotes that we can refresh the token in the future.
                IsAuthenticated = true;

                mainTaskSemaphore.Release();
            };

            // Starts to listen using the auth endpoint.
            Auth.Start();

            var authString = Auth.CreateUri();

            // This will do for now, but I'd like to have a better way of doing this.
            // Logging it as an error makes it email the code to the provided user.
            logger.LogError($"Spotify user is not authenticated. Please visit this link to authenticate:\n {authString}");
        }

        public async Task RunTokenRefreshAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            logger.LogInformation("Refreshing authentication token.");
            var newToken = await Auth.RefreshToken(RefreshToken);

            // We get one refresh token per authentication. We need to hang on to it.
            newToken.RefreshToken = RefreshToken;

            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ISpotifyContext>();

                // Keeping it simple and only keeping one token at a time.
                foreach (var token in context.Tokens)
                {
                    context.Tokens.Remove(token);
                }
                await context.Tokens.AddAsync(newToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task UpdateTokensAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Syncing Tokens with DB.");
            using (var context = contextProvider.GenerateContext<ISpotifyContext>(scopeFactory))
            {
                var token = context.Tokens.FirstOrDefault();
                var newToken = new Token
                {
                    AccessToken = token.AccessToken,
                    TokenType = token.TokenType,
                    ExpiresIn = token.ExpiresIn,
                    RefreshToken = token.RefreshToken,
                    Error = token.Error,
                    ErrorDescription = token.ErrorDescription,
                    CreateDate = token.CreateDate
                };
                Token = newToken;
                RefreshToken = token.RefreshToken;
            }
        }

        public async Task RunSortAsync(SemaphoreSlim mainTaskSemaphore, CancellationToken cancellationToken)
        {
            await mainTaskSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            await UpdateTokensAsync(cancellationToken);
            Api.GenerateApi(Token.TokenType, Token.AccessToken);

            // Stores the ID/Description association.
            ConcurrentDictionary<string, string> playlistDescriptions = new ConcurrentDictionary<string, string>();

            // Stores the PlaylistID/SongID relation for the multi call back to the API.
            ConcurrentDictionary<string, List<string>> playlistNewSongs = new ConcurrentDictionary<string, List<string>>();
            List<Task> tasks = new List<Task>();
            var playlists = GetUserPlaylistsAsync(cancellationToken);
            var likedSongs = GetUserLikedTracksAsync(cancellationToken);

            await Task.WhenAll(new Task[] { playlists, likedSongs });

            // Creating the association between the playlist ID and description
            foreach(var playlist in playlists.Result.Items)
            {
                playlistDescriptions[playlist.Id] = await GetPlaylistDescriptionsAsync(playlist.Id, cancellationToken);
                playlistNewSongs[playlist.Id] = new List<string>();
            }

            // Leveraging implicit conversion in this iteration to save on iterations elsewhere.
            foreach(SavedTrackWithGenre song in likedSongs.Result.Items)
            {
                // Storing as genre track. Paging object isn't super helpful for this.
                var songWithGenres = await GetGenreFromSongAsync(song, cancellationToken);

                // Don't need to save the modified song anywhere IFF we can just directly add it to the dict.
                await AddSongsToGenreDictionaryAsync(
                    songWithGenres,
                    playlistDescriptions,
                    playlistNewSongs,
                    cancellationToken);
            }

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
            Paging<SimplePlaylist> playlists = await Api.GetUserPlaylistsAsync(userId, options.MaxPlaylistResults);
            RequestSemaphore.Release();
            return playlists;
        }

        private async Task<PrivateProfile> GetUserAsync(CancellationToken cancellationToken)
        {
            await RequestSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation($"Requesting {user?.Id ?? "user"}'s Private profile.");
            PrivateProfile profile = await Api.GetPrivateProfileAsync();
            RequestSemaphore.Release();
            return profile;
        }

        public async Task<Paging<SavedTrack>> GetUserLikedTracksAsync(CancellationToken cancellationToken)
        {
            await RequestSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation($"Getting {user?.Id ?? "User"}'s Liked tracks.");
            Paging<SavedTrack> tracks = await Api.GetSavedTracksAsync(options.MaxSongResults);
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
            FullPlaylist fullPlaylist = await Api.GetPlaylistAsync(playlist);
            RequestSemaphore.Release();
            return fullPlaylist.Description;
        }

        public async Task<SavedTrackWithGenre> GetGenreFromSongAsync(
            SavedTrackWithGenre genreTrack,
            CancellationToken cancellationToken)
        {
            await RequestSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            List<string> genres = new List<string>();

            foreach(var artist in genreTrack.Track.Artists)
            {
                // Could async this too if performance is really taking a hit, but it probably isn't necessary.
                logger.LogInformation($"Getting Genres for artist {artist} for song {genreTrack.Track.Name}");

                // This call specifically seems to give me trouble when trying to leverage async.
                var fullArtist = await Api.GetArtistAsync(artist.Id);

                if (fullArtist.Genres != null)
                {
                    genres = genres.Concat(fullArtist.Genres).ToList();
                }
            }

            genreTrack.Genres = genres;

            cancellationToken.ThrowIfCancellationRequested();
            RequestSemaphore.Release();
            return genreTrack;
        }

        private async Task AddSongsToGenreDictionaryAsync(
            SavedTrackWithGenre genreTrack,
            ConcurrentDictionary<string, string> genreIdDict,
            ConcurrentDictionary<string, List<string>> newSongsDict,
            CancellationToken cancellationToken)
        {
            await RequestSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation($"Adding {genreTrack.Track.Name} to genreList");

            foreach(string genre in genreTrack.Genres)
            {
                foreach(KeyValuePair<string, string> genres in genreIdDict)
                {
                    // If the genre exists in any playlist genre, AND isn't already in the newSongsDict.
                    if (genres.Value.Contains(genre) && (!newSongsDict[genres.Key].Contains(genre)))
                    {
                        newSongsDict[genres.Key].Add(genreTrack.Track.Id);
                    }
                }
            }

            RequestSemaphore.Release();
        }

        private async Task MoveNewSongsIntoPlaylistsAsync(
            ConcurrentDictionary<string, List<string>> newPlaylistSongsDict,
            CancellationToken cancellationToken)
        {
            await RequestSemaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            List<Task> tasks = new List<Task>();
            List<string> unlikeList = new List<string>();

            foreach(KeyValuePair<string, List<string>> entry in newPlaylistSongsDict)
            {
                logger.LogInformation($"Moving tracks to playlist: {entry.Key}");
                tasks.Add(Api.AddPlaylistTracksAsync(entry.Key, entry.Value));
                unlikeList = unlikeList.Concat(entry.Value).ToList();
            }

            await Task.WhenAll(tasks);
            logger.LogInformation("Unliking moved songs.");
            await Api.RemoveSavedTracksAsync(unlikeList);
            RequestSemaphore.Release();
        }
    }
}