using System;
using System.Threading.Tasks;
using Geohash;
using Microsoft.Extensions.Configuration;
using TravelAdvisor.Application.Interfaces;
using TravelAdvisor.Application.Models;


namespace TravelAdvisor.Application.Services
{
    public class ClimateService : IClimateService
    {
        private readonly IApplicatonLogger<ClimateService> _logger;
        private readonly IWeatherService _weatherService;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;
        

        public static int CacheExpiryInSeconds = 10;

        public ClimateService(IApplicatonLogger<ClimateService> logger, IWeatherService weatherService,
            ICacheService cacheService, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            CacheExpiryInSeconds = int.TryParse(_configuration["CacheExpiry"], out CacheExpiryInSeconds) ? CacheExpiryInSeconds : 10;
        }

        public async Task<OpenWeather> GetClimate(double lat, double lng)
        {
            _logger.LogInformation($"Retrieving climate for [{lat} , {lng}]");
            try
            {
                var hasher = new Geohasher();
                var locationHash = hasher.Encode(lat, lng);

                var weather = new OpenWeather();
                
                var cacheExpiry = new TimeSpan(0, 0, CacheExpiryInSeconds);
                
                weather = await _cacheService.GetOrSet<OpenWeather>(locationHash,
                    () =>
                    {
                        var decoded = hasher.Decode(locationHash);
                        var lat = decoded.Item1;
                        var lng = decoded.Item2;
                        return _weatherService.GetWeather(lat, lng);
                    },
                    cacheExpiry);
                
                if (weather is null)
                {
                    return await Task.FromResult<OpenWeather>(null);
                }
                
                // var Response = new WeatherForecast
                // {
                //     Date = DateTime.Now,
                //     TemperatureC = weather.main.temp,
                //     Summary = weather.weather[0].description
                // };
                return weather;
            }
            catch (Exception e)
            {
               _logger.LogError(e.Message);
                throw;
            }
            
        }
    }

   
}