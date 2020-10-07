using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHub.SpotifySort
{
    /// <summary>
    /// Provides context to background services to avoid scoping issues in dependency
    /// injection systems.
    /// Acts as a mockable wrapper to avoid having to test on extension methods.
    /// </summary>
    public class ContextProvider : IContextProvider
    {
        /// <summary>
        /// Where T is the type of Context interface we want to generate.
        /// T has to be registered as a dependency in the service container
        /// Example: ISpotifyContext AND SpotifyContext are both added in the AddDbContext.
        /// </summary>
        /// <param name="scopeFactory"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GenerateContext<T>(IServiceScopeFactory scopeFactory)
        {
            var context = scopeFactory.CreateScope().ServiceProvider.GetService<T>();
            return context;
        }
    }
}