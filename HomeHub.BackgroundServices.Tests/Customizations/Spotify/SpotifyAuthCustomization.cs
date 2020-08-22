using AutoFixture;
using HomeHub.BackgroundServices.Configuration.SpotifySort;

namespace HomeHub.BackgroundServices.Tests.Customizations.Spotify
{
    public class SpotifyAuthCustomization : ICustomization
    {
        readonly string clientId;
        readonly string clientSecret;

        public SpotifyAuthCustomization(string clientId = "ABCD", string clientSecret = "WXYZ")
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        public SpotifyAuthentication GenerateAuthentication()
        {
            var auth = new SpotifyAuthentication
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            return auth;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Register<SpotifyAuthentication>(GenerateAuthentication);
        }
    }
}