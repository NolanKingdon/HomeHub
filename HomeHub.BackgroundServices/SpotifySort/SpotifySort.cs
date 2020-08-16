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
        readonly ILogger<SpotifySort> logger;
        public bool IsAuthenticated { get; set; }
        private SpotifyWebAPI api;

        public SpotifySort(ILogger<SpotifySort> logger)
        {
            this.logger = logger;
        }

        public async Task AuthenticateUserAsync(string clientId,
                                                string clientSecret,
                                                SemaphoreSlim semaphore,
                                                CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting authentication process.");
            Scope[] scopes = new Scope[]{
                Scope.UserLibraryModify,
                Scope.UserReadPrivate,
                Scope.UserReadEmail,
                Scope.UserLibraryRead,
                Scope.PlaylistModifyPublic
            };

            SpotifyAuthorizationCodeAuth auth = new SpotifyAuthorizationCodeAuth(
                clientId,
                clientSecret,
                "http://localhost:4002",
                "http://localhost:4002",
                scopes
            );

            auth.AuthReceived += async (sender, payload) =>
            {
                logger.LogInformation("Authentication received. Creating api object.");
                auth.Stop();

                Token token = await auth.ExchangeCode(payload.Code);
                api = new SpotifyWebAPI()
                {
                    TokenType = token.TokenType,
                    AccessToken = token.AccessToken
                };

                IsAuthenticated = true;

                semaphore.Release();
            };

            auth.Start();

            var authString = auth.CreateUri();

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}