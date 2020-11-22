using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HomeHub.Web.Middleware
{
    public class IpAuthenticationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<IpAuthenticationMiddleware> logger;
        private readonly string safelist;

        public IpAuthenticationMiddleware(
            RequestDelegate next,
            ILogger<IpAuthenticationMiddleware> logger,
            string safelist)
        {
            this.next = next;
            this.logger = logger;
            this.safelist = safelist;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            string[] validIps = safelist.Split(';');
            bool badIp = true;

            foreach (var address in validIps)
            {
                var regex = new Regex(address);
                // Check for matches.
                if (regex.IsMatch(address))
                {
                    logger.LogInformation($"Request received from valid IP Address {address}.");
                    badIp = false;
                    break;
                }
            }

            if (badIp)
            {
                logger.LogError($"Invalid IP Address attempted access to HomeHub. Address {context.Connection.RemoteIpAddress}");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            await next.Invoke(context);
        }
    }
}
