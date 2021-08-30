using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TravelAdvisor.Application.Services;
using Xunit;
using Xunit.Abstractions;

namespace TravelAdvisor.Tests
{
    public class WeatherConditionsServiceTests: BaseTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        
        private IHttpClientFactory _httpClientFactory;
        private ServiceCollection _services;
        private ServiceProvider _provider;
        
        
        private readonly WeatherConditionsService _sut;
        
        public WeatherConditionsServiceTests()
        {
             
        }
        
        [Fact]
        public async Task WeatherConditionsService_should_return_valid_temperatures() {
            
            
        }
        
    }
}