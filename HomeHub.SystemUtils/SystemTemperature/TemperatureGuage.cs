using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HomeHub.SystemUtils.Configuration;
using Microsoft.Extensions.Options;

namespace HomeHub.SystemUtils.SystemTemperature
{
    /// <summary>
    /// Reads the temperature off of the hardware and converts it into desired unit.
    /// Temperature comes to us in celcius
    /// </summary>
    public class TemperatureGuage : ITemperatureGuage
    {
        private TemperatureOptions options;
        public TemperatureGuage(IOptions<TemperatureOptions> options)
        {
            this.options = options.Value;
        }

        public async Task<double> GetSystemTemperatureAsync()
        {
            double output;
            // Create process and return result as a double.

            using (Process process = new Process())
            {
                // Configuration of the new process.
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = options.CommandFile;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;

                // Start process.
                process.Start();

                // Collect the output.
                output = Double.Parse(await process.StandardOutput.ReadToEndAsync());
                process.WaitForExit();
            }

            switch (options.Unit)
            {
                default:
                case Temperature.Celcius:
                    return ConvertTemperature(output);
                case Temperature.Fahrenheit:
                    return CelciusToFahrenheit(output);
                case Temperature.Kelvin:
                    return CelciusToKelvin(output);
            }
        }

        /// <summary>
        /// Converts the temperature read into Celcius. The temperature seems to come to us as C + E3
        ///     IE -> 20.0E3 -> 20000
        /// </summary>
        /// <returns>double - celcius conversion</returns>
        private double ConvertTemperature(double systemTemp)
        {
            return systemTemp / 1000;
        }

        private double CelciusToFahrenheit(double systemTemp)
        {
            double celcius = ConvertTemperature(systemTemp);
            return (celcius * (9/5)) + 32;
        }

        private double CelciusToKelvin(double systemTemp)
        {
            double celcius = ConvertTemperature(systemTemp);
            return celcius + 273.15;
        }
    }
}