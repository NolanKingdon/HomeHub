using SpotifyAPI.Web.Models;

namespace HomeHub.BackgroundServices.Models
{
    public class SpotifyToken
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public string Error { get; set; }

        public static implicit operator SpotifyToken(Token token)
        {
            return new SpotifyToken
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                TokenType = token.TokenType,
                Error = token.Error
            };
        }
    }
}