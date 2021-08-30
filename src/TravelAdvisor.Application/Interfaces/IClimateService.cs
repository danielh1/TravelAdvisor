using System.Threading.Tasks;
using TravelAdvisor.Application.Models;

namespace TravelAdvisor.Application.Interfaces
{
    public interface IClimateService
    {
        Task<OpenWeather> GetClimate(double lat, double lng);
    }
}