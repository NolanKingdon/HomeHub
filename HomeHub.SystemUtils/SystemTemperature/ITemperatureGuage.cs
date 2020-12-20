using System.Threading.Tasks;

namespace HomeHub.SystemUtils.SystemTemperature
{
    public interface ITemperatureGuage
    {
        Task<double> GetSystemTemperatureAsync();
    }
}