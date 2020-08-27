using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public DatabaseController(ILogger<DatabaseController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("spotify/genres")]
        public ActionResult UnsortedSpotifyGenres()
        {
            logger.LogInformation("Received Genres request.");

            // Access SpotifyContext
            // Grab Credentials
            // Create Spotify API object? (Or use a singleton we make available and update that?)
            // Make request
            // Return result

            return Ok(new
            {
                Result = "Success"
            });
        }
    }
}
