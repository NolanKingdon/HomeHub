using System;
using HomeHub.SpotifySort.Configuration;
using HomeHub.SpotifySort.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHub.SpotifySort.Extensions
{
    public static class SpotifyExtensionMethods
    {
        /// <summary>
        /// Extension method for using the SpotifySort object.
        /// Provides all necessary configurations.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection UseSpotifySorter(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return AddAndConfigureClasses(services, configuration);
        }

        /// <summary>
        /// If you want to use the library as a hosted Service,
        /// use this extension method. Will hook it up as a hosted service
        /// into the services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
       public static IServiceCollection UseSpotifySorterBackgroundService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services = AddAndConfigureClasses(services, configuration);
            services.AddHostedService<SpotifySortWorker>();

            ConfigureSpotifyDatabase();

            return services;
        }

        private static void ConfigureSpotifyDatabase()
        {
            // TODO - This is hackey and would be nice to have a static method in SpotifyContext, but it works for now.
            using SpotifyContext context = new();
            context.Database.EnsureCreated();
        }

        /// <summary>
        /// Sets up the base dependency injection and DB context for the library.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns>IServiceCollection</returns>
        private static IServiceCollection AddAndConfigureClasses(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ISpotifyContext, SpotifyContext>();
            services.AddOptions<SpotifySortOptions>()
                    .Bind(configuration.GetSection("SpotifySortOptions"));

            services.AddOptions<SpotifyAuthentication>()
                    .Bind(configuration.GetSection("SpotifyAuthentication"));

            services.AddSingleton<ISpotifySort, SpotifySorter>();
            services.AddSingleton<IApi, ApiWrapper>();
            services.AddSingleton<IContextProvider, ContextProvider>();

            return services;
        }
    }
}
