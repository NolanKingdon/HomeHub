using HomeHub.SystemUtils.Configuration;
using HomeHub.SystemUtils.SystemStorage;
using HomeHub.SystemUtils.SystemTemperature;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHub.SystemUtils.Extensions
{
    public static class SystemUtilsExtensionMethods
    {
        public static IServiceCollection UseSystemUtils(this IServiceCollection services,
                                                            IConfiguration configuration)
        {
            // Temperature
            services.AddOptions<TemperatureOptions>()
                    .Bind(configuration.GetSection("TemperatureGuage"));

            services.AddSingleton<ITemperatureGuage, TemperatureGuage>();

            // System Storage
            services.AddOptions<StorageOptions>()
                    .Bind(configuration.GetSection("SystemStorage"));

            services.AddScoped<ISystemStore, StorageHelper>();

            return services;
        }
    }
}
