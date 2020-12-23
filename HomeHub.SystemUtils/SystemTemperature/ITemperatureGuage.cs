using System.Threading.Tasks;
using HomeHub.SystemUtils.Models;

namespace HomeHub.SystemUtils.SystemTemperature
{
    public interface ITemperatureGuage
    {
        Task<TemperatureResult> GetSystemTemperatureAsync();
    }
}
