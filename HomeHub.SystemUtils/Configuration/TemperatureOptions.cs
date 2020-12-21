using HomeHub.SystemUtils.SystemTemperature;

namespace HomeHub.SystemUtils.Configuration
{
    public class TemperatureOptions
    {
        public Temperature Unit { get; set; }
        public string CommandInterface { get; set; }
        public string CommandArgs { get; set; }
    }
}