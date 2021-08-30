using System.Collections.Generic;
using System.Threading.Tasks;
using TravelAdvisor.Application.Models;

namespace TravelAdvisor.Application.Interfaces
{
    public interface IWeatherService
    {
        Task<OpenWeather> GetWeather(double lat, double lng);
        public Task<IReadOnlyCollection<OpenWeather>> GetForecasts(IReadOnlyList<Coordinates> coords);
    }
}