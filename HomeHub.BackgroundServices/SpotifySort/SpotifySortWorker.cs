using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly SpotifySortOptions options;
        public SpotifySortWorker(ILogger<SpotifySortWorker> logger,
                                 IOptions<SpotifySortOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("SpotifySortWorker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(options.Interval, stoppingToken);
            }
        }
    }
}
