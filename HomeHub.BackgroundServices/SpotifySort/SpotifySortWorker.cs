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
                    await RunSorterAsync(cancellationToken);
                }
                else
                {
                    logger.LogInformation("Previous task did not complete.");
                }
            }
        }

        private async Task RunSorterAsync(CancellationToken cancellationToken)
        {
            // Note -> Because we only have one at a time, this is probably redundant.
            await semaphore.WaitAsync();
            cancellationToken.ThrowIfCancellationRequested();

            if (!sorter.IsAuthenticated)
            {
                await sorter.AuthenticateUserAsync(authOptions.ClientId,
                                                   authOptions.ClientSecret,
                                                   localIp,
                                                   semaphore,
                                                   cancellationToken);
            }
            else
            {
                logger.LogInformation("Already authenticated");
                semaphore.Release();
            }
            // Figure out multiple semaphore situation.
            // Figure out how to send the URL via Email? Api call? SSH visible?
        }
    }
}
