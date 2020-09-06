using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeHub.Api.Dto;
using HomeHub.BackgroundServices;
using HomeHub.BackgroundServices.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeHub.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class DatabaseController : ControllerBase
    {
        private readonly ILogger<DatabaseController> logger;
        private readonly ISpotifyContext spotifyContext;
        private readonly ISpotifySort spotifySorter;

        public DatabaseController(ILogger<DatabaseController> logger, ISpotifyContext spotifyContext, ISpotifySort spotifySorter)
        {
            this.logger = logger;
            this.spotifyContext = spotifyContext;
            this.spotifySorter = spotifySorter;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("spotify/genres/")]
        [Route("spotify/genres/count")]
        public async Task<ActionResult> UnsortedSpotifyGenres()
        {
            logger.LogInformation("Received Genres request.");

            CancellationToken cancellationToken = default;
            GenreCountDto genreCount = new GenreCountDto();
            // Dictionary<string, int> genreCount = new Dictionary<string, int>();
            var token = await spotifyContext.Tokens.FirstOrDefaultAsync();

            spotifySorter.Api.GenerateApi(token.TokenType, token.AccessToken);

            // TODO -> Make a standalone re-authentication method without relying on potentially uncreated .Auth
            // In theory, this will never be an issue, because the background service is always running.
            var tracks = await spotifySorter.GetUserLikedTracksAsync(cancellationToken);

            if (tracks.Items == null)
            {
                return NotFound("No Tracks found.");
            }

            foreach (SavedTrackWithGenre track in tracks.Items)
            {
                // Todo -> Async this. ConcurrentDictionary?
                var genreTrack = await spotifySorter.GetGenreFromSongAsync(track, cancellationToken);

                foreach (string genre in genreTrack.Genres)
                {
                    genreCount.TotalCount++;
                    if(genreCount.GenreCounts.TryGetValue(genre, out int _))
                    {
                        genreCount.GenreCounts[genre]++;
                    }
                    else
                    {
                        genreCount.GenreCounts[genre] = 1;
                    }
                }
            }

            return Ok(genreCount);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("spotify/genres/detailed")]
        public async Task<ActionResult> SpotifyGenresWithSongs()
        {
            logger.LogInformation("Received Genres request.");

            CancellationToken cancellationToken = default;
            var token = await spotifyContext.Tokens.FirstOrDefaultAsync();
            var resultList = new List<DescriptiveGenresDto>();

            spotifySorter.Api.GenerateApi(token.TokenType, token.AccessToken);

            // TODO -> Make a standalone re-authentication method without relying on potentially uncreated .Auth
            // In theory, this will never be an issue, because the background service is always running.
            var tracks = await spotifySorter.GetUserLikedTracksAsync(cancellationToken);

            if (tracks.Items == null)
            {
                return NotFound("No items found.");
            }

            foreach (SavedTrackWithGenre track in tracks.Items)
            {
                // Todo -> Async this. ConcurrentDictionary?
                var genreTrack = await spotifySorter.GetGenreFromSongAsync(track, cancellationToken);
                resultList.Add(new DescriptiveGenresDto
                {
                    Artist = genreTrack.Track.Artists[0].Name,
                    SongName = genreTrack.Track.Name,
                    Genres = genreTrack.Genres
                });
            }

            return Ok(resultList);
        }
    }
}
