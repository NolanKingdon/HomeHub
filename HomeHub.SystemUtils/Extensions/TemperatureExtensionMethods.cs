using HomeHub.SystemUtils.Configuration;
using HomeHub.SystemUtils.SystemTemperature;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHub.SystemUtils.Extensions
{
    public static class TemperatureExtensionMethods
    {
        public static IServiceCollection UseTemperatureUtil(this IServiceCollection services,
                                                            IConfiguration configuration)
        {
            // Options.
            services.AddOptions<TemperatureOptions>()
                    .Bind(configuration.GetSection("TemperatureGuage"));

            // DI.
            services.AddSingleton<ITemperatureGuage, TemperatureGuage>();

            return services;
        }
    }
}
