using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using HomeHub.SpotifySort;
using HomeHub.SpotifySort.Database;
using HomeHub.SpotifySort.Models;
using HomeHub.Tests.Customizations.Spotify;
using HomeHub.Web.Controllers;
using HomeHub.Web.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using SpotifyAPI.Web.Models;
using Xunit;

namespace HomeHub.Tests
{
    public class SorterTests
    {
        readonly IFixture fixture;

        public SorterTests()
        {
            fixture = new Fixture();

            fixture.Customize(new AutoMoqCustomization())
                   .Customize(new SpotifyContextCustomizations())
                   .Customize(new SpotifyAuthCustomization())
                   .Customize(new SpotifySortOptionsCustomization())
                   .Customize(new SpotifyApiCustomization())
                   .Customize(new SpotifySorterCustomization());
        }

        // Returns Unsorted Genres
        [Fact]
        public async Task GetsUnsortedGenresAsync()
        {
            var controller = fixture.Build<SorterController>().OmitAutoProperties().Create();
            var unsortedGenres = await controller.GetUnsortedGenresAsync();

            Assert.IsType<OkObjectResult>(unsortedGenres);

            var result = (OkObjectResult)unsortedGenres;

            Assert.True(result.StatusCode == StatusCodes.Status200OK);
            Assert.True(result.Value is GenreCountDto);
            Assert.Equal(3, ((GenreCountDto)result.Value).TotalCount);
            Assert.Equal(3, ((GenreCountDto)result.Value).GenreCounts.Count);
        }

        // Returns error if not active
        [Fact]
        public async Task GetsUnsortedGenresNotActiveAsync()
        {
            fixture.Freeze<Mock<ISpotifySort>>()
                   .Setup( s => s.Active)
                   .Returns(false);

            var controller = fixture.Build<SorterController>().OmitAutoProperties().Create();
            var unsortedGenres = await controller.GetUnsortedGenresAsync();

            Assert.IsType<OkObjectResult>(unsortedGenres);

            var result = (OkObjectResult)unsortedGenres;

            Assert.True(result.StatusCode == StatusCodes.Status200OK);
            Assert.IsType<ErrorDto>(result.Value);
        }

        // Returns error if no auth
        [Fact]
        public async Task GetsUnsortedGenresNoAuthAsync()
        {
            // Setting up tokens to be empty.
            List<SpotifyToken> dbsetReference = new List<SpotifyToken>();
            var mock = dbsetReference.AsQueryable().BuildMockDbSet();

            fixture.Freeze<Mock<ISpotifyContext>>()
                   .Setup(sc => sc.Tokens)
                   .Returns(() => mock.Object);

            var controller = fixture.Build<SorterController>().OmitAutoProperties().Create();
            var unsortedGenres = await controller.GetUnsortedGenresAsync();

            Assert.IsType<OkObjectResult>(unsortedGenres);
            var result = (OkObjectResult)unsortedGenres;
            Assert.True(result.StatusCode == StatusCodes.Status200OK);
            Assert.IsType<ErrorDto>(result.Value);
        }

        // Returns 404 if no tracks found
        [Fact]
        public async Task GetsUnsortedGenresNoGenresFoundAsync()
        {
            var sorter = fixture.Freeze<Mock<ISpotifySort>>();
            sorter.Setup( s => s.GetUserLikedTracksAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new Paging<SavedTrack>());

            var controller = fixture.Build<SorterController>().OmitAutoProperties().Create();
            var unsortedGenres = await controller.GetUnsortedGenresAsync();

            Assert.IsType<NotFoundObjectResult>(unsortedGenres);
            var result = (NotFoundObjectResult)unsortedGenres;
            Assert.True(result.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task GetsSpotifyGenresWithSongsAsync()
        {
            var controller = fixture.Build<SorterController>().OmitAutoProperties().Create();
            var unsortedGenres = await controller.SpotifyGenresWithSongsAsync();

            Assert.IsType<OkObjectResult>(unsortedGenres);

            var result = (OkObjectResult)unsortedGenres;

            Assert.True(result.StatusCode == StatusCodes.Status200OK);
            Assert.True(result.Value is List<DescriptiveGenresDto>);
            Assert.True(((List<DescriptiveGenresDto>)result.Value).Count == 1);
        }

        // Returns error if not activated
        [Fact]
        public async Task GetsSpotifyGenresWithSongsNotActiveAsync()
        {
            List<SpotifyToken> dbsetReference = new List<SpotifyToken>();
            var mock = dbsetReference.AsQueryable().BuildMockDbSet();

            fixture.Freeze<Mock<ISpotifyContext>>()
                   .Setup(sc => sc.Tokens)
                   .Returns(() => mock.Object);

            var controller = fixture.Build<SorterController>().OmitAutoProperties().Create();
            var unsortedGenres = await controller.SpotifyGenresWithSongsAsync();

            Assert.IsType<OkObjectResult>(unsortedGenres);

            var result = (OkObjectResult)unsortedGenres;

            Assert.True(result.StatusCode == StatusCodes.Status200OK);
            Assert.IsType<ErrorDto>(result.Value);
        }

        // Returns error if no auth
        [Fact]
        public async Task GetsSpotifyGenresWithSongsNoAuthAsync()
        {
            // Setting up tokens to be empty.
            List<SpotifyToken> dbsetReference = new List<SpotifyToken>();
            var mock = dbsetReference.AsQueryable().BuildMockDbSet();

            fixture.Freeze<Mock<ISpotifyContext>>()
                   .Setup(sc => sc.Tokens)
                   .Returns(() => mock.Object);

            var controller = fixture.Build<SorterController>().OmitAutoProperties().Create();
            var unsortedGenres = await controller.SpotifyGenresWithSongsAsync();

            Assert.IsType<OkObjectResult>(unsortedGenres);
            var result = (OkObjectResult)unsortedGenres;
            Assert.True(result.StatusCode == StatusCodes.Status200OK);
            Assert.IsType<ErrorDto>(result.Value);
        }

        // Returns 404 if no songs are found
        [Fact]
        public async Task GetsSpotifyGenresWithSongsNoItemsAsync()
        {
            var sorter = fixture.Freeze<Mock<ISpotifySort>>();
            sorter.Setup( s => s.GetUserLikedTracksAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new Paging<SavedTrack>());

            var controller = fixture.Build<SorterController>().OmitAutoProperties().Create();
            var unsortedGenres = await controller.SpotifyGenresWithSongsAsync();

            Assert.IsType<NotFoundObjectResult>(unsortedGenres);
            var result = (NotFoundObjectResult)unsortedGenres;
            Assert.True(result.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task GetsSpotifyStatusAsync()
        {
            var controller = fixture.Build<SorterController>().OmitAutoProperties().Create();
            var result = await controller.GetSpotifyStatusAsync();

            Assert.IsType<OkObjectResult>(result);
            var resultObject = (OkObjectResult)result;
            Assert.Equal(resultObject.StatusCode, StatusCodes.Status200OK);
            Assert.IsType<StatusUpdateDto>(resultObject.Value);

            // Defaults to False, but is setup as True for testing purposes.
            Assert.True(((StatusUpdateDto)resultObject.Value).Status);
        }

        [Fact]
        public async Task GetsSpotifyStatusExceptionAsync()
        {
            fixture.Freeze<Mock<ISpotifySort>>()
                   .Setup( ss => ss.Active)
                   .Throws(new Exception("This really should not have happened."));

            var controller = fixture.Build<SorterController>().OmitAutoProperties().Create();
            var result = await controller.GetSpotifyStatusAsync();

            Assert.IsType<BadRequestObjectResult>(result);
            var resultObject = (BadRequestObjectResult)result;
            Assert.Equal(resultObject.StatusCode, StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task TogglesSorterAsync()
        {
             var controller = fixture.Build<SorterController>().OmitAutoProperties().Create();
            var result = await controller.ToggleSorterAsync();

            Assert.IsType<OkObjectResult>(result);
            var resultObject = (OkObjectResult)result;
            Assert.Equal(resultObject.StatusCode, StatusCodes.Status200OK);
            Assert.IsType<StatusUpdateDto>(resultObject.Value);

            // Defaults to False, but is setup as True for testing purposes.
            Assert.False(((StatusUpdateDto)resultObject.Value).Status);
        }

        [Fact]
        public async Task TogglesSorterExceptionAsync()
        {
        fixture.Freeze<Mock<ISpotifySort>>()
               .Setup( ss => ss.Active)
               .Throws(new Exception("This really should not have happened."));

            var controller = fixture.Build<SorterController>().OmitAutoProperties().Create();
            var result = await controller.ToggleSorterAsync();

            Assert.IsType<BadRequestObjectResult>(result);
            var resultObject = (BadRequestObjectResult)result;
            Assert.Equal(resultObject.StatusCode, StatusCodes.Status400BadRequest);
        }
    }
}
