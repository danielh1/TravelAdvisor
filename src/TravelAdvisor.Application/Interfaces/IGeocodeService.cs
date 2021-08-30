using System.Threading.Tasks;
using TravelAdvisor.Application.Models;

namespace TravelAdvisor.Application.Interfaces
{
    public interface IGeocodeService
    {
        public Task<Geocode> GeocodeAddressAsync(string address);
    }
}