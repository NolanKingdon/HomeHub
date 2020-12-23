using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using HomeHub.SystemUtils.Models;
using HomeHub.SystemUtils.SystemTemperature;
using HomeHub.Tests.Customizations.System.TemperatureGuage;
using HomeHub.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HomeHub.Tests
{
    public class SystemControllerTests
    {
        private readonly IFixture fixture;

        public SystemControllerTests()
        {
            // Creating IoC container.
            fixture = new Fixture();

            // Adding DI definitions.
            fixture.Customize(new AutoMoqCustomization());
        }

        // -- Temperature Tests --

        /// <summary>
        /// Happy Path for testing the proper functionality of the temperature guage.
        /// </summary>
        /// <param name="unit">Unit to convert to</param>
        /// <param name="input">System temperature read</param>
        /// <param name="expected">Expected result based on input</param>
        [Theory]
        [InlineData(Temperature.Celcius, 40000, 40)]
        [InlineData(Temperature.Fahrenheit, 40000, 104)]
        [InlineData(Temperature.Kelvin, 40000, 313.15)]
        public async Task GetTemperatureTestAsync(Temperature unit, double input, double expected)
        {
            fixture.Customize(new TemperatureGuageCustomization(unit, input))
                   .Customize(new TemperatureOptionsCustomization(unit));

            SystemController mockController = fixture.Build<SystemController>()
                                                     .OmitAutoProperties()
                                                     .Create();

            var result = await mockController.SystemTemperatureAsync();

            Assert.IsType<OkObjectResult>(result);
            OkObjectResult okResult = (OkObjectResult) result;

            Assert.IsType<TemperatureResult>(okResult.Value);
            TemperatureResult tempResult = (TemperatureResult)okResult.Value;

            Assert.Equal(expected, tempResult.Temperature);
            Assert.Equal(unit.ToString(), tempResult.Unit);
        }

        [Fact]
        public async Task GetGemperatureTestFailAsync()
        {
            fixture.Customize(new TemperatureOptionsCustomization());

            fixture.Freeze<Mock<ITemperatureGuage>>()
                   .Setup(tg => tg.GetSystemTemperatureAsync())
                   .Throws(new Exception());

            SystemController mockController = fixture.Build<SystemController>()
                                                     .OmitAutoProperties()
                                                     .Create();

            var result = await mockController.SystemTemperatureAsync();

            Assert.IsType<BadRequestObjectResult>(result);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;
            Assert.IsType<Exception>(badResult.Value);
        }
    }
}
