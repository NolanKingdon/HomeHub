using HomeHub.BackgroundServices.Configuration.SpotifySort;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomeHub.BackgroundServices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args);

            var host = builder.UseConsoleLifetime()
                              .Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Starting Background Services.");

            host.Run();

            logger.LogInformation("Gracefully stopping Background Services.");
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, configBuilder) =>
                {
                    configBuilder.AddJsonFile("./secrets.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // TODO -> As number of Services grows, hide these away in extension methods.
                    services.AddHostedService<SpotifySortWorker>()
                            .AddOptions<SpotifySortOptions>()
                            .Bind(hostContext.Configuration.GetSection(nameof(SpotifySortOptions)));

                    services.AddOptions<SpotifyAuthentication>()
                            .Bind(hostContext.Configuration.GetSection(nameof(SpotifyAuthentication)));

                    services.AddSingleton<ISpotifySort, SpotifySort>();
                });

            return hostBuilder;
        }
    }
}
