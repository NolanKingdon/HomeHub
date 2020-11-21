using System;
using System.Threading;
using System.Threading.Tasks;
using HomeHub.SpotifySort.Configuration;
using SpotifyAPI.Web.Models;

namespace HomeHub.SpotifySort
{
    public interface ISpotifySort
    {
        bool IsAuthenticated { get; set; }
        SpotifyAuthorizationCodeAuth Auth { get; set; }
        string RefreshToken { get; set; }
        Token Token { get; set; }
        IApi Api { get; }
        bool Active { get; set; }

        Task AuthenticateUserAsync(string clientId,
                                          string clientSecret,
                                          string localIp,
                                          SemaphoreSlim semaphore,
                                          CancellationToken cancellationToken);

        Task RunTokenRefreshAsync(CancellationToken cancellationToken);
        Task RunSortAsync(SemaphoreSlim semaphore, CancellationToken cancellationToken);
        Task<Paging<SavedTrack>> GetUserLikedTracksAsync(CancellationToken cancellationToken);
        Task<SavedTrackWithGenre> GetGenreFromSongAsync(
            SavedTrackWithGenre genreTrack,
            CancellationToken cancellationToken);
    }
}
