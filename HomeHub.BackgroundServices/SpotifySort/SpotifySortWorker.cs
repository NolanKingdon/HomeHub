using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeHub.BackgroundServices.Configuration.SpotifySort;
using Microsoft.Extensions.Configuration;
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
        public SpotifySortWorker(ILogger<SpotifySortWorker> logger,
                                 IOptions<SpotifySortOptions> sortOptions,
                                 IOptions<SpotifyAuthentication> authOptions,
                                 ISpotifySort sorter,
                                 IConfiguration config)
        {
            this.logger = logger;
            this.sortOptions = sortOptions.Value;
            this.authOptions = authOptions.Value;
            this.sorter = sorter;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation($"Options: {sortOptions}");
                logger.LogInformation($"Options.SpotifyAuth: {sortOptions.SpotifyAuthentication}");
                logger.LogInformation("SpotifySortWorker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(sortOptions.Interval, cancellationToken);
            }
        }
    }
}
