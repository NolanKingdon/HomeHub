using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeHub.BackgroundServices;
using HomeHub.BackgroundServices.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [Route("spotify/genres")]
        public async Task<ActionResult> UnsortedSpotifyGenres()
        {
            logger.LogInformation("Received Genres request.");
            
            CancellationToken cancellationToken = default;
            Dictionary<string, int> genreCount = new Dictionary<string, int>();
            var token = spotifyContext.Tokens.FirstOrDefault();

            spotifySorter.Api.GenerateApi(token.TokenType, token.AccessToken);

            // TODO -> Make a standalone re-authentication method without relying on potentially uncreated .Auth
            // In theory, this will never be an issue, because the background service is always running.
            var tracks = await spotifySorter.GetUserLikedTracksAsync(cancellationToken);

            foreach (SavedTrackWithGenre track in tracks.Items)
            {
                var genreTrack = await spotifySorter.GetGenreFromSongAsync(track, cancellationToken);

                foreach (string genre in genreTrack.Genres)
                {
                    if (genreCount.TryGetValue(genre, out _))
                    {
                        genreCount[genre] ++;
                    }
                    else
                    {
                        genreCount.Add(genre, 1);
                    }
                }
            }

            // TODO -> An official DTO.
            return Ok(new
            {
                Result = genreCount
            });
        }
    }
}
