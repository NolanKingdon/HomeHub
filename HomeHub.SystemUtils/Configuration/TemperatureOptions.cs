using HomeHub.SystemUtils.SystemTemperature;

namespace HomeHub.SystemUtils.Configuration
{
    public class TemperatureOptions
    {
        public Temperature Unit { get; set; }

        // Program that will run the commands. (IE. QTerminal, cmd...)
        public string CommandInterface { get; set; }

        // Commands passed as args string. (IE. "ECHO 50000")
        public string CommandArgs { get; set; }
    }
}
