using System;
using Microsoft.Extensions.Logging;

namespace HomeHub.BackgroundServices
{
    public class SpotifySort : ISpotifySort
    {
        readonly ILogger<SpotifySort> logger;
        public SpotifySort(ILogger<SpotifySort> logger)
        {
            this.logger = logger;
        }
    }
}