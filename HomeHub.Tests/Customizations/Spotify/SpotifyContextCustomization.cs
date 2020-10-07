using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using HomeHub.SpotifySort;
using HomeHub.SpotifySort.Configuration;
using HomeHub.SpotifySort.Database;
using HomeHub.SpotifySort.Models;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using SpotifyAPI.Web.Models;

namespace HomeHub.Tests.Customizations.Spotify
{
    public class SpotifyContextCustomizations : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            // Leveraging implicit conversion to use autofixture create.
            Token test = fixture.Create<Token>();
            List<SpotifyToken> dbsetReference = new List<SpotifyToken>() { test };
            var mock = dbsetReference.AsQueryable().BuildMockDbSet();
            var mockContext = fixture.Freeze<Mock<ISpotifyContext>>();
            mockContext.Setup(mc => mc.Tokens)
                       .Returns(mock.Object);

            var provider = fixture.Freeze<Mock<IContextProvider>>();
            provider.Setup(p => p.GenerateContext<ISpotifyContext>(It.IsAny<IServiceScopeFactory>()))
                    .Returns(mockContext.Object);
        }
    }
}