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

            cancellationToken.ThrowIfCancellationRequested();
        }

        public async Task RunSortAsync(SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync();

            var playlists = await GetUserPlaylistsAsync(semaphore, cancellationToken);

            foreach (var list in playlists.Items)
            {
                logger.LogInformation(list.Id);
            }

            semaphore.Release();
        }

        public async Task<Paging<SimplePlaylist>> GetUserPlaylistsAsync(SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            PrivateProfile user = await GetUserAsync(cancellationToken);
            string userId = user.Id;
            Paging<SimplePlaylist> playlists = await api.GetUserPlaylistsAsync(userId, 50);

            return playlists;
        }

        private async Task<PrivateProfile> GetUserAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PrivateProfile profile = await api.GetPrivateProfileAsync();
            return profile;
        }
    }
}