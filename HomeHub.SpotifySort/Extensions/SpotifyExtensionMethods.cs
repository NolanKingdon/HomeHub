using System;
using HomeHub.SpotifySort.Configuration;
using HomeHub.SpotifySort.Database;
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
        /// <param name="sortOptions">SpotifySortOptions action</param>
        /// <param name="authOptions">SpotifyAuthenticationOptions action</param>
        /// <returns></returns>
        public static IServiceCollection UseSpotifySorter(
            this IServiceCollection services,
            Action<SpotifySortOptions> sortOptions = null,
            Action<SpotifyAuthentication> authOptions = null)
        {
            return AddAndConfigureClasses(services, sortOptions, authOptions);
        }


        /// <summary>
        /// If you want to use the library as a hosted Service,
        /// use this extension method. Will hook it up as a hosted service
        /// into the services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="sortOptions"></param>
        /// <param name="authOptions"></param>
        /// <returns></returns>
        public static IServiceCollection UseSpotifySorterBackgroundService(
            this IServiceCollection services,

            Action<SpotifySortOptions> sortOptions = null,
            Action<SpotifyAuthentication> authOptions = null)
        {
            services = AddAndConfigureClasses(services, sortOptions, authOptions);

            // TODO -> This may require us to pass in the host builder. It's here for if/when it happens.
            services.AddHostedService<SpotifySortWorker>()
                    .AddOptions(sortOptions);

            return services;
        }

    /// <summary>
    /// Sets up the base DI and database for the SpotifySort library.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="sortOptions"></param>
    /// <param name="authOptions"></param>
    /// <returns></returns>
        private static IServiceCollection AddAndConfigureClasses(
            IServiceCollection services,
            Action<SpotifySortOptions> sortOptions,
            Action<SpotifyAuthentication> authOptions)
        {
            services.AddDbContext<ISpotifyContext, SpotifyContext>();

            services.AddOptions<SpotifySortOptions>()
                    .Configure(sortOptions);

            if (authOptions != null)
            {
                services.AddOptions<SpotifyAuthentication>()
                        .Configure(authOptions);
            }

            services.AddSingleton<ISpotifySort, SpotifySort>();
            services.AddSingleton<IApi, ApiWrapper>();

            return services;
        }
    }
}
