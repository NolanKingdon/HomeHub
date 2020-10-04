using System;
using System.Text;
using System.Text.RegularExpressions;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;

namespace HomeHub.SpotifySort
{
    public class SpotifyAuthorizationCodeAuth : AuthorizationCodeAuth
    {
        private readonly Regex capsRegex = new Regex("(?<!^)(?=[A-Z])");
        public string ScopeUri { get; set; }
        public SpotifyAuthorizationCodeAuth(string clientId,
                                            string clientSecret,
                                            string redirectUri,
                                            string serverUri,
                                            Scope[] scopes,
                                            string state = "")
        : base(clientId, clientSecret, redirectUri, serverUri, Scope.None, state)
        {
            // Will create bitwise shifted scopes for base and the scopes for the URI for my purposes here.
            GenerateRequiredScopeObjects(scopes);
        }

        public string CreateUri()
        {
            return $"https://accounts.spotify.com/en/authorize/?client_id={ClientId}&response_type=code&redirect_uri={RedirectUri.Replace("/", "%2F")}&state={State}&scope={ScopeUri}&show_dialog={ShowDialog}";
        }

        private void GenerateRequiredScopeObjects(Scope[] scopes)
        {
            Scope bitwiseScopes = 0;

            for(int i=0; i<scopes.Length; i++)
            {
                string enumName = Enum.GetName(typeof(Scope), scopes[i]);

                // For SpotifyAPI library to run the receiving server.
                bitwiseScopes |= scopes[i];

                // For me to hand the URL off to where it needs to go for my addin.
                ScopeUri += string.Join("-", capsRegex.Split(enumName)).ToLower();

                if(i != scopes.Length-1)
                {
                    ScopeUri += "%20";
                }
            }

            base.Scope = bitwiseScopes;
        }
    }
}