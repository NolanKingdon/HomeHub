namespace HomeHub.SystemUtils.SystemTemperature
{
    public static class TemperatureConverter
    {
        /// <summary>
        /// Converts the temperature read into Celcius. The temperature seems to come to us as C + E3
        ///     IE -> 20.0E3 -> 20000
        /// </summary>
        /// <returns>double - celcius conversion</returns>
        public static double SystemTempToCelcius(double systemTemp)
        {
            return systemTemp / 1000;
        }

        public static double SystemTempToFahrenheit(double systemTemp)
        {
            double celcius = SystemTempToCelcius(systemTemp);
            return (celcius * 9/5) + 32;
        }

        public static double SystemTempToKelvin(double systemTemp)
        {
            double celcius = SystemTempToCelcius(systemTemp);
            return celcius + 273.15;
        }
    }
}