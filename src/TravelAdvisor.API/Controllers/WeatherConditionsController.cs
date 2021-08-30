using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Geohash;
using Microsoft.AspNetCore.Mvc;
using TravelAdvisor.Application.Interfaces;
using TravelAdvisor.Application.Models;

namespace TravelAdvisor.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class WeatherConditionsController  : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly IClimateService _climateService;
        private readonly ICacheService _cacheService;
        private readonly IGetWeatherConditionsQuery _weatherConditionsQuery;


        public WeatherConditionsController(IWeatherService weatherService, IClimateService climateService,
            ICacheService cacheService, IGetWeatherConditionsQuery weatherConditionsQuery)
        {
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _climateService = climateService ?? throw new ArgumentNullException(nameof(climateService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _weatherConditionsQuery =
                weatherConditionsQuery ?? throw new ArgumentNullException(nameof(weatherConditionsQuery));
        }

        [HttpGet("conditions")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<WeatherForecast>> Get([FromQuery] String Latitude , String Longitude)
        {
            if (String.IsNullOrEmpty(Latitude)|string.IsNullOrEmpty(Longitude))
            {
                return BadRequest();
            }

            try
            {
                double latitude = double.Parse(Latitude);
                double longitude = double.Parse(Longitude);

                 
                var hasher = new Geohasher();
                var locationHash = hasher.Encode(latitude, longitude);

                var weather = new OpenWeather();
                var cacheExpiry = new TimeSpan(0, 0, 10);



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
                    return StatusCode(404);
                }
                
                var Response = new WeatherForecast
                {
                    Date = DateTime.Now,
                    TemperatureC = weather.main.temp,
                    Summary = weather.weather[0].description
                };
                return Ok(Response);
            }
            catch (ArgumentException ae)
            {
                return Problem(ae.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        
        
        
        [HttpGet("climate")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<OpenWeather>> GetClimate([FromQuery] String Latitude , String Longitude)
        {
            
            if (String.IsNullOrEmpty(Latitude)|string.IsNullOrEmpty(Longitude))
            {
                return BadRequest();
            }
            try
            {
                double latitude = double.Parse(Latitude);
                double longitude = double.Parse(Longitude);

                var hasher = new Geohasher();
                var locationHash = hasher.Encode(latitude, longitude);

                var weather = new OpenWeather();
                var cacheExpiry = new TimeSpan(0, 0, 10);
                weather = await _cacheService.GetOrSet<OpenWeather>(locationHash,
                    () =>
                    {
                        var decoded = hasher.Decode(locationHash);
                        var lat = decoded.Item1;
                        var lng = decoded.Item2;
                        return _climateService.GetClimate(lat, lng);
                    },
                    cacheExpiry);
                
                if (weather is null)
                {
                    return StatusCode(404);
                }
               
                return Ok(weather);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPost("BulkForecasts")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(object), (int) HttpStatusCode.OK)]
         public async Task<ActionResult<List<Climate>>> GetBulkWeatherForecasts ([FromBody] List<Coordinates> locations) {
   
            List<string> listData = new List<string>();
            IReadOnlyList<string> readOnlyData = listData.AsReadOnly();

            try
            {
                // var weather = locations.Coordinates;
                // IReadOnlyList<Coordinates> coords = weather.AsReadOnly();
                
                var weather = locations;
                IReadOnlyList<Coordinates> coords = weather.AsReadOnly();
                
                var res =  await _weatherConditionsQuery.GetForecasts(coords);
                
                if (res is null)
                {
                    return StatusCode(404);
                }
                
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
            
        }

    }
    
   
}