using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using HomeHub.Tests.Customizations.Spotify;
using HomeHub.SpotifySort;
using HomeHub.SpotifySort.Configuration;
using Moq;
using Xunit;
using HomeHub.SpotifySort.Database;
using HomeHub.SpotifySort.Models;
using SpotifyAPI.Web.Models;
using System.Linq;
using MockQueryable.Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace HomeHub.Tests
{
    /// <summary>
    /// Some basic tests. Not super comprehensive, just the bare minimum for what I would consider
    /// to be 'working'. I don't test for re-attempts because of the short lived lifetime of the
    /// application - retrying in an hour more than fits the need.
    /// </summary>
    public class SpotifyServiceTestToPass
    {
        readonly IFixture fixture;
        readonly SpotifySorter sort;

        public SpotifyServiceTestToPass()
        {
            fixture = new Fixture();

            fixture.Customize(new AutoMoqCustomization())
                   .Customize(new SpotifyAuthCustomization())
                   .Customize(new SpotifySortOptionsCustomization())
                   .Customize(new SpotifyApiCustomization())
                   .Customize(new SpotifyContextCustomizations());

            // Autofixture takes care of dependencies for us.
            sort = fixture.Create<SpotifySorter>();
        }

        // TODO -> Learn how to invoke on an event listener so the test doesn't hang.
        // [Fact]
        // public async Task AuthenticatesUserAsyncAsync()
        // {
        //     var clientId = fixture.Create<string>();
        //     var clientSecret = fixture.Create<string>();
        //     var localIp = fixture.Create<string>();
        //     var semaphore = new SemaphoreSlim(1,1);
        //     CancellationToken token = default;

        //     await sort.AuthenticateUserAsync(
        //         clientId,
        //         clientSecret,
        //         localIp,
        //         semaphore,
        //         token
        //     );

        //     Assert.True(sort.Auth != null, "Spotify AuthorizationCode is null or empty.");
        //     Assert.True(sort.IsAuthenticated, "Authentication not completed.");
        // }

        [Fact]
        public async Task GetsPlaylistsAsync()
        {
            var semaphore = new SemaphoreSlim(1, 1);
            CancellationToken token = default;
            var api = fixture.Freeze<Mock<IApi>>();
            var options = fixture.Create<SpotifySortOptions>();

            await sort.RunSortAsync(semaphore, token);
            api.Verify( a => a.GetUserPlaylistsAsync(It.IsAny<string>(), options.MaxPlaylistResults), Times.Once);
        }

        [Fact]
        public async Task GetsLikedTracksAsync()
        {
            var semaphore = new SemaphoreSlim(1, 1);
            CancellationToken token = default;
            var api = fixture.Freeze<Mock<IApi>>();
            var options = fixture.Create<SpotifySortOptions>();

            await sort.RunSortAsync(semaphore, token);
            api.Verify( a => a.GetSavedTracksAsync(options.MaxSongResults), Times.Once);
        }

        [Fact]
        public async Task GetsPlaylistDescriptionsAsync()
        {
            var semaphore = new SemaphoreSlim(1, 1);
            CancellationToken token = default;
            var api = fixture.Freeze<Mock<IApi>>();
            var options = fixture.Create<SpotifySortOptions>();

            await sort.RunSortAsync(semaphore, token);
            api.Verify( a => a.GetPlaylistAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetsGenresAsync()
        {
            var semaphore = new SemaphoreSlim(1, 1);
            CancellationToken token = default;
            var api = fixture.Freeze<Mock<IApi>>();
            var options = fixture.Create<SpotifySortOptions>();

            await sort.RunSortAsync(semaphore, token);
            api.Verify( a => a.GetArtistAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task MovesLikedSongsToPlaylistAsync()
        {
            var semaphore = new SemaphoreSlim(1, 1);
            CancellationToken token = default;
            var api = fixture.Freeze<Mock<IApi>>();
            var options = fixture.Create<SpotifySortOptions>();

            await sort.RunSortAsync(semaphore, token);
            api.Verify( a => a.GetSavedTracksAsync(options.MaxSongResults), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RemovesMovedSongsFromLikedAsync()
        {
            var semaphore = new SemaphoreSlim(1, 1);
            CancellationToken token = default;
            var api = fixture.Freeze<Mock<IApi>>();
            var options = fixture.Create<SpotifySortOptions>();

            await sort.RunSortAsync(semaphore, token);
            api.Verify( a => a.AddPlaylistTracksAsync(It.IsAny<string>(), It.IsAny<Collection<string>>()), Times.AtLeastOnce);
            api.Verify( a => a.RemoveSavedTracksAsync(It.IsAny<Collection<string>>()), Times.Once);
        }
    }
}
