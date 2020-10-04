using System;
using System.Threading;
using System.Threading.Tasks;
using HomeHub.BackgroundServices.Configuration.SpotifySort;
using SpotifyAPI.Web.Models;

namespace HomeHub.SpotifySort
{
    public interface ISpotifySort
    {
        public bool IsAuthenticated { get; set; }
        public SpotifyAuthorizationCodeAuth Auth { get; set; }
        public string RefreshToken { get; set; }
        public Token Token { get; set; }
        public IApi Api { get; }

        public Task AuthenticateUserAsync(string clientId,
                                          string clientSecret,
                                          string localIp,
                                          SemaphoreSlim semaphore,
                                          CancellationToken cancellationToken);

        Task RunTokenRefreshAsync(CancellationToken cancellationToken);
        public Task RunSortAsync(SemaphoreSlim semaphore, CancellationToken cancellationToken);
        Task<Paging<SavedTrack>> GetUserLikedTracksAsync(CancellationToken cancellationToken);
        Task<SavedTrackWithGenre> GetGenreFromSongAsync(
            SavedTrackWithGenre genreTrack,
            CancellationToken cancellationToken);
    }
}