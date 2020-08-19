using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeHub.BackgroundServices.Configuration.SpotifySort;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace HomeHub.BackgroundServices
{
    public class SpotifySort : ISpotifySort
    {
        private readonly ILogger<SpotifySort> logger;
        private SpotifyWebAPI api;
        private PrivateProfile user;
        public bool IsAuthenticated { get; set; }
        public SpotifyAuthorizationCodeAuth Auth { get; set; }
        public string RefreshToken { get; set; }
        public Token Token { get; set; }

        public SpotifySort(ILogger<SpotifySort> logger)
        {
            this.logger = logger;
        }

        public async Task AuthenticateUserAsync(string clientId,
                                                string clientSecret,
                                                string localIp,
                                                SemaphoreSlim semaphore,
                                                CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Starting authentication process.");

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

            Auth.AuthReceived += async (sender, payload) =>
            {
                logger.LogInformation("Authentication received. Creating api object.");
                Auth.Stop();

                Token = await Auth.ExchangeCode(payload.Code);
                var refreshToken = Token.RefreshToken;

                api = new SpotifyWebAPI()
                {
                    TokenType = Token.TokenType,
                    AccessToken = Token.AccessToken
                };

                // Denotes that we can refresh the token in the future.
                IsAuthenticated = true;

                semaphore.Release();
            };

            Auth.Start();

            var authString = Auth.CreateUri();

            logger.LogInformation($"Please visit this link to authenticate: {authString}");
        }

        public async Task RunSortAsync(SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();

            // Stores the ID/Description association.
            Dictionary<string, string> playlistDescriptions = new Dictionary<string, string>();

            // Stores the PlaylistID/SongID relation for the multi call back to the API.
            Dictionary<string, List<string>> playlistNewSongs = new Dictionary<string, List<string>>();
            var playlists = GetUserPlaylistsAsync(cancellationToken);
            var likedSongs = GetUserLikedTracksAsync(cancellationToken);
            List<Task> tasks = new List<Task>();

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

            // Multicall for adding to playlists/removing from liked.
            await MoveNewSongsIntoPlaylistsAsync(playlistNewSongs, cancellationToken);

            semaphore.Release();
        }

        private async Task<Paging<SimplePlaylist>> GetUserPlaylistsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            user ??= await GetUserAsync(cancellationToken);
            string userId = user.Id;
            logger.LogInformation($"Getting playlists from {userId}.");
            Paging<SimplePlaylist> playlists = await api.GetUserPlaylistsAsync(userId, 100);
            return playlists;
        }

        private async Task<PrivateProfile> GetUserAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
                logger.LogInformation($"Requesting {user?.Id ?? "user"}'s Private profile.");
            PrivateProfile profile = await api.GetPrivateProfileAsync();
            return profile;
        }

        private async Task<Paging<SavedTrack>> GetUserLikedTracksAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation($"Getting {user?.Id ?? "User"}'s Liked tracks.");
            Paging<SavedTrack> tracks = await api.GetSavedTracksAsync(100);
            cancellationToken.ThrowIfCancellationRequested();
            return tracks;
        }

        private async Task<string> GetPlaylistDescriptionsAsync(
            string playlist,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation($"Getting Playlist description for playlistID: {playlist}");
            FullPlaylist fullPlaylist = await api.GetPlaylistAsync(playlist, "", "");
            return fullPlaylist.Description;
        }

        private async Task<SavedTrackWithGenre> GetGenreFromSongAsync(
            SavedTrackWithGenre genreTrack,
            CancellationToken cancellationToken)
        {
            foreach(var artist in genreTrack.Track.Artists)
            {
                // Could async this too if performance is really taking a hit, but it probably isn't necessary.
                logger.LogInformation($"Getting Genres for artist {artist} for song {genreTrack.Track.Name}");
                var fullArtist = await api.GetArtistAsync(artist.Id);
                genreTrack.Genres = genreTrack.Genres.Concat(fullArtist.Genres).ToList();
            }

            cancellationToken.ThrowIfCancellationRequested();
            return genreTrack;
        }

        private async Task AddSongsToGenreDictionaryAsync(
            SavedTrackWithGenre genreTrack,
            Dictionary<string, string> genreIdDict,
            Dictionary<string, List<string>> newSongsDict,
            CancellationToken cancellationToken)
        {
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
        }
    
        private async Task MoveNewSongsIntoPlaylistsAsync(
            Dictionary<string, List<string>> newPlaylistSongsDict,
            CancellationToken cancellationToken)
        {
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
        }
    }
}