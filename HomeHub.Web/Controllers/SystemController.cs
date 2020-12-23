using System;
using System.Threading.Tasks;
using HomeHub.SystemUtils.Configuration;
using HomeHub.SystemUtils.Models;
using HomeHub.SystemUtils.SystemTemperature;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeHub.Web.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class SystemController : ControllerBase
    {
        private readonly ILogger<SystemController> logger;
        private readonly ITemperatureGuage temperatureGuage;
        private readonly TemperatureOptions temperatureOptions;

        public SystemController(ILogger<SystemController> logger,
                                ITemperatureGuage temperatureGuage,
                                IOptions<TemperatureOptions> temperatureOptions)
        {
            this.logger = logger;
            this.temperatureGuage = temperatureGuage;
            this.temperatureOptions = temperatureOptions.Value;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("temperature")]
        public async Task<ActionResult> SystemTemperatureAsync()
        {
            logger.LogInformation("Received Temps request.");

            try
            {
                TemperatureResult temperature = await temperatureGuage.GetSystemTemperatureAsync();
                string unit = temperatureOptions.Unit.ToString();

                return Ok(temperature);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("vpn/{action}")]
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
