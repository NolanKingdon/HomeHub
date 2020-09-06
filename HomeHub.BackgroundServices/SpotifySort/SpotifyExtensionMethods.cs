using System;
using HomeHub.BackgroundServices.Configuration.SpotifySort;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHub.BackgroundServices
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
