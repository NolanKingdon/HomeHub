using HomeHub.BackgroundServices.Configuration.SpotifySort;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomeHub.BackgroundServices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // TODO -> As number of Services grows, hide these away in extension methods.
                    services.AddHostedService<SpotifySortWorker>();

                    services.AddOptions();
                    services.AddOptions<SpotifySortOptions>()
                            .Bind(hostContext.Configuration.GetSection(nameof(SpotifySortOptions)));
                });
    }
}
