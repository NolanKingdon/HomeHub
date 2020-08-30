using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HomeHub.BackgroundServices.Configuration.SpotifySort;
using HomeHub.BackgroundServices.Database;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory scopeFactory;

        public SpotifySortWorker(ILogger<SpotifySortWorker> logger,
                                 IOptions<SpotifySortOptions> sortOptions,
                                 IOptions<SpotifyAuthentication> authOptions,
                                 ISpotifySort sorter,
                                 IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            this.sortOptions = sortOptions.Value;
            this.authOptions = authOptions.Value;
            this.sorter = sorter;
            this.localIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList[3].ToString();
            this.semaphore = new SemaphoreSlim(1, 1);
            this.scopeFactory = scopeFactory;
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
                        semaphore.Release();
                    }
                    catch(Exception e)
                    {
                        logger.LogCritical($"Unexpected Error when Running SpotifySort background service:\n{e}");

                        // Clearing up the semaphore for a future attempt.
                        semaphore.Release();
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
                    var newToken = await sorter.Auth.RefreshToken(sorter.RefreshToken);

                    using (var scope = scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetService<ISpotifyContext>();
                        // Keeping it simple and only keeping one token at a time.
                        foreach (var token in context.Tokens)
                        {
                            context.Tokens.Remove(token);
                        }
                        context.Tokens.Add(newToken);
                        context.SaveChanges();
                    }
                }

                await sorter.RunSortAsync(semaphore, cancellationToken);
            }

            // TODO -> Error handling and request re-attempts w/ Polly.
            logger.LogInformation("Sort successful!");
        }
    }
}
