using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HomeHub.SpotifySort;
using HomeHub.SpotifySort.Database;
using HomeHub.Web.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeHub.Web.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class SorterController : ControllerBase
    {
        private readonly ILogger<SorterController> logger;
        private readonly ISpotifyContext spotifyContext;
        private readonly ISpotifySort spotifySorter;

        public SorterController(ILogger<SorterController> logger, ISpotifyContext spotifyContext, ISpotifySort spotifySorter)
        {
            this.logger = logger;
            this.spotifyContext = spotifyContext;
            this.spotifySorter = spotifySorter;
        }

        /// <summary>
        /// Returns a dictionary where the Key is the genre type, and the value is how
        /// many times the genre occurs in my liked songs.
        /// Is designed to be polled to see if a new playlist should be created to accomodate the
        /// influx of the new songs or not.
        /// </summary>
        /// <remarks>
        ///     Sample Request
        ///         GET - https://localhost:5001/api/v1/database/spotify/genres
        /// </remarks>
        /// <returns><see ref="GenreCountDto"></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("genres/")]
        [Route("genres/count")]
        public async Task<ActionResult> GetUnsortedGenresAsync()
        {
            logger.LogInformation("Received Genres request.");

            if (!spotifySorter.Active)
            {
                return Ok(new ErrorDto
                {
                    Error = "SpotifySorter is currently offline. Please use activation request to start the service."
                });
            }

            CancellationToken cancellationToken = default;
            GenreCountDto genreCount = new GenreCountDto();
            var token = await spotifyContext.Tokens.FirstOrDefaultAsync();

            if (spotifySorter.Api == null || token == null)
            {
                return Ok(new ErrorDto
                {
                    Error = "SpotifySorter API Does not exist. Authentication may be required."
                });
            }

            // TODO -> Throw an error response if doesn't have authentication yet.
            spotifySorter.Api.GenerateApi(token.TokenType, token.AccessToken);

            // TODO -> Make a standalone re-authentication method without relying on potentially uncreated .Auth
            // In theory, will never be an issue, because the background service is always running.
            var tracks = await spotifySorter.GetUserLikedTracksAsync(cancellationToken);

            if (tracks.Items == null)
            {
                return NotFound("No Tracks found.");
            }

            foreach (SavedTrackWithGenre track in tracks.Items)
            {
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

        /// <summary>
        /// Returns a list of DescriptiveGenresDto.
        /// Intended use is to be called after the count endpoint to see the association between
        /// song and genre. Lets you investigate strange genres.
        /// </summary>
        /// <remarks>
        ///     Sample request:
        ///         GET https://localhost:5001/api/v1/database/spotify/genres/detailed
        /// </remarks>
        /// <returns>List of DescriptiveGenresDto objects</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("genres/detailed")]
        public async Task<ActionResult> SpotifyGenresWithSongsAsync()
        {
            if (!spotifySorter.Active)
            {
                return Ok(new
                {
                    Error = "SpotifySorter is currently offline. Please use activation request to start the service."
                });
            }

            // Todo -> Add ability to sort by specific song/genres?
            logger.LogInformation("Received Genres request.");

            CancellationToken cancellationToken = default;
            var token = await spotifyContext.Tokens.FirstOrDefaultAsync();
            var resultList = new List<DescriptiveGenresDto>();

            if (spotifySorter.Api == null || token == null)
            {
                return Ok(new ErrorDto
                {
                    Error = "SpotifySorter API Does not exist. Authentication may be required."
                });
            }

            spotifySorter.Api.GenerateApi(token.TokenType, token.AccessToken);

            var tracks = await spotifySorter.GetUserLikedTracksAsync(cancellationToken);

            if (tracks.Items == null)
            {
                return NotFound("No items found.");
            }

            foreach (SavedTrackWithGenre track in tracks.Items)
            {
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("status/")]
        public async Task<ActionResult> GetSpotifyStatusAsync()
        {
            try
            {
                StatusUpdateDto status = new StatusUpdateDto()
                {
                    Status = spotifySorter.Active
                };

                return Ok(status);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("status/toggle")]
        public async Task<ActionResult> ToggleSorterAsync()
        {
            try
            {
                bool newStatus = !spotifySorter.Active;
                spotifySorter.Active = newStatus;
                StatusUpdateDto status = new StatusUpdateDto()
                {
                    Status = newStatus
                };

                return Ok(status);
            }
            catch (Exception e)
            {
                // Going to throw the whole error because I'm the only one using this.
                return BadRequest(e);
            }
        }
    }
}
