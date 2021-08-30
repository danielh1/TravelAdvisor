using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Geohash;
using Microsoft.Extensions.Configuration;
using TravelAdvisor.Application.Interfaces;
using TravelAdvisor.Application.Models;

namespace TravelAdvisor.Application.Services
{
    public class WeatherConditionsService : IGetWeatherConditionsQuery
    {
        
        private readonly ICacheService _cacheService;
        private readonly IApplicatonLogger<ClimateService> _logger;
        private readonly IConfiguration _configuration;
        private readonly OpenWeatherMapHttpClient _openWeatherMapHttpClient;
        private readonly IWeatherService _weatherService;
        
        public static int MaxConcurrency;
        public static int CacheExpiryInSeconds = 10;
        


        public WeatherConditionsService(ICacheService cacheService, IApplicatonLogger<ClimateService> logger,
            IConfiguration configuration, OpenWeatherMapHttpClient openWeatherMapHttpClient,
            IWeatherService weatherService)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _openWeatherMapHttpClient = openWeatherMapHttpClient ??
                                        throw new ArgumentNullException(nameof(openWeatherMapHttpClient));
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            
            MaxConcurrency = int.TryParse(_configuration["OpenWeatherMapMaxConcurrentConnections"], out MaxConcurrency) ? MaxConcurrency : 10; 
            CacheExpiryInSeconds = int.TryParse(_configuration["CacheExpiry"], out CacheExpiryInSeconds) ? CacheExpiryInSeconds : 10;
        }


        public async Task<IReadOnlyCollection<OpenWeather>> GetForecasts(IReadOnlyList<Coordinates> coordinates)
        {
            var semaphoreSlim = new SemaphoreSlim(MaxConcurrency, MaxConcurrency);
            var responses = new ConcurrentBag<OpenWeather>();

            var tasks = coordinates.Select(async coordinate =>
            {
                
                await semaphoreSlim.WaitAsync();
                try
                {
                    var hasher = new Geohasher();
                    var locationHash = hasher.Encode(coordinate.lat, coordinate.lng);

                    var weather = new OpenWeather();
                
                    var cacheExpiry = new TimeSpan(0, 0, CacheExpiryInSeconds);
                    
                    weather = await _cacheService.GetOrSet<OpenWeather>(locationHash,
                        () =>
                        {
                            var decoded = hasher.Decode(locationHash);
                            var lat = decoded.Item1;
                            var lng = decoded.Item2;
                           // return _openWeatherMapHttpClient.GetForecast(coordinate.lat, coordinate.lng);
                           return _weatherService.GetWeather(coordinate.lat, coordinate.lng);
                        },
                        cacheExpiry);
                        responses.Add(weather);
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            });
            await Task.WhenAll(tasks);
            return responses.Select(x => x).ToArray();
        }
        
        
        
        
    }
}