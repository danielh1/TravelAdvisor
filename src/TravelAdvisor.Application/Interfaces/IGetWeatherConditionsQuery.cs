using System.Collections.Generic;
using System.Threading.Tasks;
using TravelAdvisor.Application.Models;

namespace TravelAdvisor.Application.Interfaces
{
    public interface IGetWeatherConditionsQuery
    {
        public Task<IReadOnlyCollection<OpenWeather>> GetForecasts(IReadOnlyList<Coordinates> coords);
    }
}