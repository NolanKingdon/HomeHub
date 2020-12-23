using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HomeHub.SystemUtils.Configuration;
using HomeHub.SystemUtils.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeHub.SystemUtils.SystemTemperature
{
    /// <summary>
    /// Reads the temperature off of the hardware and converts it into desired unit.
    /// Temperature comes to us in celcius
    /// </summary>
    public class TemperatureGuage : ITemperatureGuage
    {
        private readonly TemperatureOptions options;
        private readonly ILogger<TemperatureGuage> logger;

        public TemperatureGuage(IOptions<TemperatureOptions> options,
                                ILogger<TemperatureGuage> logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }

        public async Task<TemperatureResult> GetSystemTemperatureAsync()
        {
            double output;
            string stdOut;
            TemperatureResult result = new TemperatureResult {Unit = options.Unit.ToString()};

            try
            {
                using (Process process = new Process())
                {
                    logger.LogInformation("Starting Temperature read process");
                    // Configuration of the new process.
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = options.CommandInterface;
                    process.StartInfo.Arguments = options.CommandArgs;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;

                    // Start process.
                    process.Start();

                    // Command returns \r\n at the end. We don't need that. 
                    stdOut = Regex.Match(await process.StandardOutput.ReadToEndAsync(), @"\d+").Value;

                    // Collect the output.
                    output = Double.Parse(stdOut);
                    process.WaitForExit();
                }

                switch (options.Unit)
                {
                    default:
                    case Temperature.Celcius:
                        result.Temperature = TemperatureConverter.SystemTempToCelcius(output);
                        break;
                    case Temperature.Fahrenheit:
                        result.Temperature = TemperatureConverter.SystemTempToFahrenheit(output);
                        break;
                    case Temperature.Kelvin:
                        result.Temperature = TemperatureConverter.SystemTempToKelvin(output);
                        break;
                }
                logger.LogInformation($"Temperature read successful, returning temperature - {result.Temperature} degrees {result.Unit} ");
                return result;
            }
            catch (OverflowException e)
            {
                // Bubble up exception after catching it here.
                logger.LogError(e, "Overflow when converting terminal result to double");
                throw;
            }
        }
    }
}
