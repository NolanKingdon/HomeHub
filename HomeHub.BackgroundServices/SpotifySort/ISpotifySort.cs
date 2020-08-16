using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeHub.BackgroundServices
{
    public interface ISpotifySort
    {
        public bool IsAuthenticated { get; set; }
        public Task AuthenticateUserAsync(string clientId,
                                          string clientSecret,
                                          string localIp,
                                          SemaphoreSlim semaphore,
                                          CancellationToken cancellationToken);
    }
}