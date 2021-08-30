using System.Threading.Tasks;
using TravelAdvisor.Application.Models;

namespace TravelAdvisor.Application.Interfaces
{
    public interface ITouringService
    {
        public Task<ApplicationMessage<Leg>>  GetDirectionsFastAsync(string origin, string destination, double minTemperature, double maxTemperature);
        
        public Task<Leg> GetDirectionsSlowAsync(string origin, string destination);
        
        
        
        
    }
}