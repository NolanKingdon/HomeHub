using Microsoft.Extensions.DependencyInjection;

namespace HomeHub.SpotifySort
{
    public interface IContextProvider
    {
        T GenerateContext<T>(IServiceScopeFactory scopeFactory);
    }
}