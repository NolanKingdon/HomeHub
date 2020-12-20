using HomeHub.SystemUtils.SystemTemperature;

namespace HomeHub.SystemUtils.Configuration
{
    public class TemperatureOptions
    {
        public Temperature Unit { get; set; }
        public string CommandFile { get; set; }
    }
}