using AutoFixture;
using HomeHub.SystemUtils.Models;
using HomeHub.SystemUtils.SystemTemperature;
using Moq;

namespace HomeHub.Tests.Customizations.System.TemperatureGuage
{
    public class TemperatureGuageCustomization : ICustomization
    {
        private readonly Temperature unit;
        private readonly double returnValue;

        public TemperatureGuageCustomization(Temperature unit = 0, double returnValue = 40000)
        {
            this.unit = unit;
            this.returnValue = returnValue;
        }

        private double GetConvertedValue(Temperature unit, double returnValue)
        {
            return unit switch
            {
                Temperature.Fahrenheit => TemperatureConverter.CelciusToFahrenheit(returnValue),
                Temperature.Kelvin => TemperatureConverter.CelciusToKelvin(returnValue),
                _ => TemperatureConverter.ConvertSystemTempToCelcius(returnValue),
            };
        }

        public void Customize(IFixture fixture)
        {
            var temperatureGuage = fixture.Freeze<Mock<ITemperatureGuage>>();

            temperatureGuage.Setup(tg => tg.GetSystemTemperatureAsync())
                            .ReturnsAsync(new TemperatureResult()
                            {
                                Temperature = GetConvertedValue(unit, returnValue),
                                Unit = unit.ToString()
                            });
        }
    }
}