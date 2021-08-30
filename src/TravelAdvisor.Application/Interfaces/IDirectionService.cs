using System.Threading.Tasks;
using TravelAdvisor.Application.Models;

namespace TravelAdvisor.Application.Interfaces
{
    public interface IDirectionService
    {
        public Task<Directions> GetDirectionsAsync(string origin, string destination, bool map);
    }
}