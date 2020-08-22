using AutoFixture;
using HomeHub.BackgroundServices.Configuration.SpotifySort;
using Microsoft.Extensions.Options;

namespace HomeHub.BackgroundServices.Tests.Customizations.Spotify
{
    public class SpotifySortOptionsCustomization : ICustomization
    {
        readonly int interval;
        readonly int maxPlaylistResults;
        readonly int maxSongResults;
        readonly int maxConcurrentThreads;

        public SpotifySortOptionsCustomization(
            int interval = 1,
            int maxPlaylistResults = 100,
            int maxSongResults = 100,
            int maxConcurrentThreads = 40)
        {
            this.interval = interval;
            this.maxPlaylistResults = maxPlaylistResults;
            this.maxSongResults = maxSongResults;
            this.maxConcurrentThreads = maxConcurrentThreads;
        }

        private SpotifySortOptions GenerateOptions()
        {
            var options = new SpotifySortOptions
            {
                Interval = interval,
                MaxPlaylistResults = maxPlaylistResults,
                MaxSongResults = maxSongResults,
                MaxConcurrentThreads = maxConcurrentThreads
            };

            return options;
        }

        private IOptions<SpotifySortOptions> GenerateIOptions()
        {
            var sortOptions = GenerateOptions();
            var options = Options.Create<SpotifySortOptions>(sortOptions);

            return options;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Register<SpotifySortOptions>(GenerateOptions);
            fixture.Register<IOptions<SpotifySortOptions>>(GenerateIOptions);
        }
    }
}