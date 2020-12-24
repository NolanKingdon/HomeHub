using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;

namespace HomeHub.SpotifySort
{
    /// <summary>
    /// Wrapper class for the API. Allows for Dependency injection and for testing
    /// with mocked responses.
    /// </summary>
    public class ApiWrapper : IApi
    {
        public SpotifyWebAPI api { get; private set; }
        public void GenerateApi(string tokenType, string accessToken)
        {
            api = new SpotifyWebAPI()
            {
                TokenType = tokenType,
                AccessToken = accessToken
            };
        }
        public Task<ErrorResponse> AddPlaylistTracksAsync(string playlistId, Collection<string> tracks)
        {
            return api.AddPlaylistTracksAsync(playlistId, tracks.ToList());
        }

        public Task<FullArtist> GetArtistAsync(string artistId)
        {
            return api.GetArtistAsync(artistId);
        }

        public Task<FullPlaylist> GetPlaylistAsync(string playlistId)
        {
            return api.GetPlaylistAsync(playlistId);
        }

        public Task<PrivateProfile> GetPrivateProfileAsync()
        {
            return api.GetPrivateProfileAsync();
        }

        public Task<Paging<SavedTrack>> GetSavedTracksAsync(int maxSongResults)
        {
            return api.GetSavedTracksAsync(maxSongResults);
        }

        public Task<Paging<SimplePlaylist>> GetUserPlaylistsAsync(string userId, int maxPlaylistResults)
        {
            return api.GetUserPlaylistsAsync(userId, maxPlaylistResults);
        }

        public Task<ErrorResponse> RemoveSavedTracksAsync(Collection<string> unlikeList)
        {
            return api.RemoveSavedTracksAsync(unlikeList.ToList());
        }
    }
}