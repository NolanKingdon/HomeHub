using System.Collections.Generic;
using System.Threading.Tasks;
using SpotifyAPI.Web.Models;

namespace HomeHub.SpotifySort
{
    public interface IApi
    {
        void GenerateApi(string tokenType, string accessToken);
        Task<Paging<SimplePlaylist>> GetUserPlaylistsAsync(string userId, int maxPlaylistResults);
        Task<PrivateProfile> GetPrivateProfileAsync();
        Task<Paging<SavedTrack>> GetSavedTracksAsync(int maxSongResults);
        Task<FullPlaylist> GetPlaylistAsync(string playlistId);
        Task<FullArtist> GetArtistAsync(string artistId);
        Task<ErrorResponse> AddPlaylistTracksAsync(string playlistId, List<string> tracks);
        Task<ErrorResponse> RemoveSavedTracksAsync(List<string> unlikeList);
    }
}