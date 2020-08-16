using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HomeHub.BackgroundServices.Configuration.SpotifySort;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeHub.BackgroundServices
{
    public class SpotifySortWorker : BackgroundService
    {
        private readonly ILogger<SpotifySortWorker> logger;
        private readonly SpotifySortOptions sortOptions;
        private readonly SpotifyAuthentication authOptions;
        private readonly ISpotifySort sorter;
        private readonly SemaphoreSlim semaphore;
        private readonly string localIp;
        public SpotifySortWorker(ILogger<SpotifySortWorker> logger,
                                 IOptions<SpotifySortOptions> sortOptions,
                                 IOptions<SpotifyAuthentication> authOptions,
                                 ISpotifySort sorter)
        {
            this.logger = logger;
            this.sortOptions = sortOptions.Value;
            this.authOptions = authOptions.Value;
            this.sorter = sorter;
            this.localIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList[3].ToString();
            this.semaphore = new SemaphoreSlim(1, 1);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(sortOptions.Interval, cancellationToken);

                if(semaphore.CurrentCount == 1)
                {
                    logger.LogInformation("Running SpotifySortWorker - {time}", DateTimeOffset.Now);
                    try
                    {
                        await RunSorterAsync(cancellationToken);
                    }
                    catch(OperationCanceledException)
                    {
                        logger.LogError("SpotifySort cancellation received. Disposing of worker.");
                    }
                    catch(Exception e)
                    {
                        logger.LogCritical($"Unexpected Error when Running SpotifySort background service:\n{e}");
                    }
                }
                else
                {
                    logger.LogInformation("Previous task did not complete.");
                }
            }
        }

        private async Task RunSorterAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!sorter.IsAuthenticated)
            {
                logger.LogInformation("Running authentication process.");
                await sorter.AuthenticateUserAsync(authOptions.ClientId,
                                                   authOptions.ClientSecret,
                                                   localIp,
                                                   semaphore,
                                                   cancellationToken);
                await sorter.RunSortAsync(semaphore, cancellationToken);
            }
            else
            {
                if(sorter.Token.IsExpired())
                {
                    logger.LogInformation("Refreshing authentication token.");
                    await sorter.Auth.RefreshToken(sorter.RefreshToken);
                }

                await sorter.RunSortAsync(semaphore, cancellationToken);
            }

            // TODO -> Error handling and request re-attempts w/ Polly.
            logger.LogInformation("Sort successful!");
        }
    }
}
