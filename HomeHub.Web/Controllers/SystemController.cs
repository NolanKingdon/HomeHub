using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HomeHub.Web.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class SystemController : ControllerBase
    {
        private readonly ILogger<SystemController> logger;

        public SystemController(ILogger<SystemController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("status/temperature")]
        public ActionResult SystemTemperature()
        {
            logger.LogInformation("Received Temps request.");

            return Ok(new
            {
                Result = "Success"
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("system/vpn/{action}")]
        public ActionResult ActivateVpn(bool? activate)
        {
            switch(activate)
            {
                case true:
                    logger.LogInformation("Activating VPN for system.");
                    break;
                case false:
                    logger.LogInformation("De-activating VPN for system.");
                    break;
                default:
                    logger.LogInformation("Invalid command received.");
                    return NotFound(new
                    {
                        Result = "Command Not Found."
                    });
            }

            return Ok(new
            {
                Result = "Success - VPN status updated."
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("download/{downloadUrl}")]
        public ActionResult DownloadWithUrl(string url)
        {
            return Ok(new
            {
                Result = "Download Started"
            });
        }
    }
}
