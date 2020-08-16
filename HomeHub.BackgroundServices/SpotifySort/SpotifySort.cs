using System.Collections.Generic;
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

            /**
            * The idea here is to be able to use a multicall to spotify to add all the new songs to the
            * Playlists, and in the same vein be able to conglomerate a big ol' list for "unlike songs" call.
            * One of the issues the node version had was that spotify couldn't handle the volume of the
            * Remove requests, we got duplicates and glitches in unliking songs.
            * This should let us only iterate through the liked songs once as well.
            */
            Dictionary<string, string[]> playlistNewSongs = new Dictionary<string, string[]>();

            // TODO -> Add these calls to an Task list and await them all before continuing.
            var playlists = await GetUserPlaylistsAsync(cancellationToken);
            var likedSongs = await GetUserLikedTracksAsync(cancellationToken);

            // Creating the association between the playlist ID and description
            foreach(var playlist in playlists.Items)
            {
                playlistDescriptions[playlist.Id] = await GetPlaylistDescriptionsAsync(playlist.Id, cancellationToken);
            }

            // Add the songs to the relevant playlists,

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
            logger.LogInformation($"Requesting user's Private profile.");
            PrivateProfile profile = await api.GetPrivateProfileAsync();
            return profile;
        }

        private async Task<Paging<SavedTrack>> GetUserLikedTracksAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation($"Getting {user.Id ?? "User"}'s Liked tracks.");
            Paging<SavedTrack> tracks = await api.GetSavedTracksAsync(100);
            return tracks;
        }

        private async Task<string> GetPlaylistDescriptionsAsync(string playlist,
                                                                CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation($"Getting Playlist description for playlistID: {playlist}");
            FullPlaylist fullPlaylist = await api.GetPlaylistAsync(playlist, "", "");
            return fullPlaylist.Description;
        }
    }
}