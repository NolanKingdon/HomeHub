using AutoFixture;
using HomeHub.SystemUtils.Configuration;
using HomeHub.SystemUtils.SystemTemperature;
using Microsoft.Extensions.Options;

namespace HomeHub.Tests.Customizations.System.TemperatureGuage
{
    public class TemperatureOptionsCustomization : ICustomization
    {
        private Temperature unit;

        public TemperatureOptionsCustomization(Temperature unit = 0)
        {
            this.unit = unit;
        }

        public void Customize(IFixture fixture)
        {
            TemperatureOptions options = new TemperatureOptions()
            {
                Unit = unit,
                CommandInterface = "Test",
                CommandArgs = "Test"
            };

            IOptions<TemperatureOptions> tempOptions = Options.Create<TemperatureOptions>(options);

            fixture.Register<IOptions<TemperatureOptions>>(() => tempOptions);
        }
    }
}