using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Geohash;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TravelAdvisor.Application.Interfaces;
using TravelAdvisor.Application.Models;



namespace TravelAdvisor.Application.Services
{
    public class WeatherService : IWeatherService
    {
        
        private readonly IConfiguration _configuration;
        private IHttpClientFactory _httpClientFactory;
        
        public static int MaxConcurrency;
        public static int CacheExpiryInSeconds = 10;
      

      public WeatherService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
      {
          _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
          _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
          
          MaxConcurrency = int.TryParse(_configuration["OpenWeatherMapMaxConcurrentConnections"], out MaxConcurrency) ? MaxConcurrency : 10; 
          CacheExpiryInSeconds = int.TryParse(_configuration["CacheExpiry"], out CacheExpiryInSeconds) ? CacheExpiryInSeconds : 10;
      }

      public async Task<OpenWeather> GetWeather(double lat, double lng)
        {
            var weather = new OpenWeather();
            var apiKey = _configuration["OpenWeatherMapKey"];

            var httpClient = _httpClientFactory.CreateClient();
            var APIURL =  $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lng}&appid={apiKey}&units=metric";
            var response =  await httpClient.GetAsync(APIURL);
            weather = JsonConvert.DeserializeObject<OpenWeather>(await response.Content.ReadAsStringAsync());
            return weather;
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
                    
                  
                  weather = 
                  
                  // await  _openWeatherMapHttpClient.GetForecast(coordinate.lat, coordinate.lng);
                   await  this.GetWeather(coordinate.lat, coordinate.lng);
                  
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