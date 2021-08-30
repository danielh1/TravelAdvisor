using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Directions.Request;
using GoogleApi.Entities.Maps.Directions.Response;
using Microsoft.Extensions.Configuration;
using TravelAdvisor.Application.Interfaces;
using TravelAdvisor.Application.Models;
using System.Linq;
using Geohash;
using GoogleApi.Entities.Common.Enums;
using Leg = TravelAdvisor.Application.Models.Leg;

namespace TravelAdvisor.Application.Services
{
    public class TouringService : ITouringService
    {
        private readonly IConfiguration _configuration;
        private readonly IClimateService _climateService;
        private readonly IGetWeatherConditionsQuery _weatherConditionsQuery;
        private readonly IApplicatonLogger<TouringService> _logger;

        public TouringService(IConfiguration configuration, IClimateService climateService,
            IGetWeatherConditionsQuery weatherConditionsQuery, IApplicatonLogger<TouringService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _climateService = climateService ?? throw new ArgumentNullException(nameof(climateService));
            _weatherConditionsQuery =
                weatherConditionsQuery ?? throw new ArgumentNullException(nameof(weatherConditionsQuery));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Leg> GetDirectionsSlowAsync(string origin, string destination)
        {
            var request = new DirectionsRequest()
            {
                Key = _configuration["GoogleApiKey"],
                Origin = new Location(origin),
                Destination = new Location(destination)
                //request.DepartureTime = UserParameters.DepartureTime;
            };
            DirectionsResponse googleResponse;
         
            googleResponse = await GoogleMaps.Directions.QueryAsync(request);
         
            //TODO : check if DirectionsResponse returns more than one leg
            
            // "you can assume that only one leg will be returned at all times (e.g. no need to handle multiple legs)."
            var TravelAdviceResponse = new Leg();
            TravelAdviceResponse.start_address = origin;
            TravelAdviceResponse.end_address = destination;

            if (!googleResponse.Routes.Any())
            {
                return null;
            }

            var firstLeg = googleResponse.Routes.First().Legs.ToList();
            
            // firstLeg.ForEach(leg =>
            // {
            //     // TravelAdviceResponse.distance.value = leg.Distance.Value;
            //     // TravelAdviceResponse.distance.text = leg.Distance.Text;
            //     TravelAdviceResponse.distance = new Distance(leg.Distance.Text, leg.Distance.Value);
            // });

            foreach (var leg in googleResponse.Routes.First().Legs)
            {
                TravelAdviceResponse.distance = new Distance(leg.Distance.Text, leg.Distance.Value);
                TravelAdviceResponse.steps = new List<TripStep>();
                TravelAdviceResponse.start_location =
                    new Coordinates(leg.StartLocation.Latitude, leg.StartLocation.Longitude);
                TravelAdviceResponse.end_location =
                    new Coordinates(leg.EndLocation.Latitude, leg.EndLocation.Longitude);

                foreach (var step in leg.Steps)
                {
                    var coords = new Coordinates(step.EndLocation.Latitude, step.EndLocation.Longitude);
                    var weatherConditions = await getWeatherAtStep(coords);
                    var hasher = new Geohasher();
                    var locHash = hasher.Encode(step.EndLocation.Latitude, step.EndLocation.Longitude);
                    var stp = new TripStep()
                    {
                        
                        html_instructions = step.HtmlInstructions.ToString(),
                        distance = new Distance(step.Distance.Text, step.Distance.Value),
                        duration = new TimeDuration(step.Duration.Text, step.Duration.Value),
                        climate = new Climate(weatherConditions.TemperatureC ,weatherConditions.Summary),
                        end_location = new Coordinates(step.EndLocation.Latitude, step.EndLocation.Longitude)
                        {
                           locationHash =  locHash
                        }
                        
                    };
                    TravelAdviceResponse.steps.Add(stp);
                }
                
            }

            var recommendVisit = getTravelAdvice(TravelAdviceResponse.steps.Select(k => k.climate));

            TravelAdviceResponse.travelAdvice = await recommendVisit;
            
            return TravelAdviceResponse;
        }

       
        private  async Task<WeatherForecast> getWeatherAtStep(Coordinates locationCoords)
        { 
            var climate = await _climateService.GetClimate(locationCoords.lat, locationCoords.lng);
            var forecast = new WeatherForecast
            {
                Date = DateTime.Now,
                TemperatureC = climate.main.temp,
                Summary = climate.weather[0].description
            };
            return forecast;
        }

        /// <summary>
        /// Function which return boolean if temperature propert of Climate object  is between a max and min value
        /// </summary>
        private Func<Climate, double, double, bool> recommendVisit = (weather, minTemp, maxTemp) => 
            ((weather.celsiusTemp >= minTemp) && (weather.celsiusTemp <= maxTemp) ) || (weather.celsiusTemp is -999);
        
        /// <summary>
        /// Predicate indicating the acceptable temperature range
        /// ignoring celsius temperatures which are out of range (ie. -999)
        /// </summary>
        private Predicate<Climate> shouldVisit = (weather) => (weather.celsiusTemp is >= 20 and <= 30 ) || (weather.celsiusTemp is -999);
    
        /// <summary>
        /// Check if there is a location whose weather forecast in not within the recommended temperature range  
        /// </summary>
        /// <param name="weatherForecasts"></param>
        /// <returns></returns>
        private  async Task<string> getTravelAdvice(IEnumerable<Climate> climates )
        {
            return climates.Any(x => !shouldVisit(x)) ? "No" : "Yes";
        }

        /// <summary>
        /// Check if there is a location whose weather forecast in not within the recommended temperature range  
        /// </summary>
        /// <param name="climates"></param>
        /// <param name="minTemp">Minimum temperature in celsius</param>
        /// <param name="maxTemp">Maximum temperature in celsius<</param>
        /// <returns></returns>
        private  async Task<string> getTravelRecommendation(IEnumerable<Climate> climates, double minTemp, double maxTemp )
        {
            return climates.Any(x => !recommendVisit(x, minTemp, maxTemp)) ? "No" : "Yes";
        }
        
        
        public async Task<ApplicationMessage<Leg>> GetDirectionsFastAsync(string origin, string destination, double minTemp, double maxTemp)
        {
            ApplicationMessage<Leg> applicationLegResult = new ApplicationMessage<Leg>();
            
            var geohasher = new Geohasher();
            var request = new DirectionsRequest()
            {
                Key = _configuration["GoogleApiKey"],
                Origin = new Location(origin),
                Destination = new Location(destination)
                //request.DepartureTime = UserParameters.DepartureTime;
            };
            DirectionsResponse googleResponse;
          
            googleResponse = await GoogleMaps.Directions.QueryAsync(request);

            if ( (googleResponse is not null)  && ((googleResponse.Status != Status.Ok)))
            {
                applicationLegResult = new ApplicationMessage<Leg>()
                {
                    ServiceSuccess = false,
                    ErrorMessage = Enum.GetName(typeof(Status), googleResponse.Status ?? Status.Undefined )
                };
                //exit early without checking weather
                return applicationLegResult;
            }
         
            // "you can assume that only one leg will be returned at all times (e.g. no need to handle multiple legs)."
            var TravelAdviceResponse = new Leg();
            TravelAdviceResponse.start_address = origin;
            TravelAdviceResponse.end_address = destination;
          
            //Iterate through "Steps" and populate TravelAdviceResponse
            foreach (var leg in googleResponse.Routes.First().Legs)
            {
                TravelAdviceResponse.distance = new Distance(leg.Distance.Text, leg.Distance.Value);
                TravelAdviceResponse.steps = new List<TripStep>();
                TravelAdviceResponse.start_location =
                    new Coordinates(leg.StartLocation.Latitude, leg.StartLocation.Longitude);
                TravelAdviceResponse.end_location =
                    new Coordinates(leg.EndLocation.Latitude, leg.EndLocation.Longitude);

                foreach (var step in leg.Steps)
                {
                   var coords = new Coordinates(step.EndLocation.Latitude, step.EndLocation.Longitude);
                   var stp = new TripStep()
                    {
                        html_instructions = step.HtmlInstructions.ToString(),
                        distance = new Distance(step.Distance.Text, step.Distance.Value),
                        duration = new TimeDuration(step.Duration.Text, step.Duration.Value),
                        end_location = new Coordinates(step.EndLocation.Latitude, step.EndLocation.Longitude, geohasher.Encode(step.EndLocation.Latitude, step.EndLocation.Longitude))
                   };
                    TravelAdviceResponse.steps.Add(stp);
                }
            }
            
            // get weather forecast for all coordinates in each "Step"
            var climates = TravelAdviceResponse.steps.Select(w => w.end_location).ToList().AsReadOnly();

            // var hashedClimates = climates.Select(k => { });

            //instantiate a Dictionary to hold Coordinates by geohashed key
            Dictionary<string, Coordinates> climateByCoordinates = new Dictionary<string, Coordinates>();
            foreach (var item in climates)
            {
                var hasher = new Geohasher();
                var locationHash = hasher.Encode(item.lat, item.lng);
                climateByCoordinates[locationHash] = item;
            }

            // get the weather forecasts from OpenWeatherMapAPI
            var weatherReadings = await _weatherConditionsQuery.GetForecasts(climates);

            // populate a Dictionary with results of the weather forecasts
            Dictionary<string, OpenWeather> weatherReadingsByCoordinates = new Dictionary<string, OpenWeather>();
            foreach (var item in weatherReadings)
            {
                var hasher = new Geohasher();
                var locationHash = hasher.Encode(item.coord.lat, item.coord.lon);
                weatherReadingsByCoordinates[locationHash] = item;
            }
            
           // TravelAdviceResponse.steps.ToList().ForEach(w => w.end_location.locationHash == weatherReadingsByCoordinates)

           // create a list of "Climate" properties and populate it with the relevant properties that were returned 
           // from the OpenWeatherMapAPI
           var TravelAdviceWithClimate = TravelAdviceResponse.steps.Select(c =>
           {
               var weatherConditions = getWeatherFromForecast(weatherReadingsByCoordinates , c.end_location.locationHash);
               c.climate = new Climate(weatherConditions.TemperatureC, weatherConditions.Summary);
               return c;
           });
           
           // TravelAdviceResponse.steps.Select(c =>
           // {
           //      c.climate.
           //     return c.;
           // }).ToList();
           //
            //todo : call these after dealing with "weatherReadings"
           // var recommendVisit = getTravelAdvice(TravelAdviceResponse.steps.Select(k => k.climate));
           // TravelAdviceResponse.travelAdvice = await recommendVisit;
           
           
           // iterate of the forecasts for climate in each step and determine whether it is recommended to visit.
           TravelAdviceResponse.steps = TravelAdviceWithClimate.ToList();
           var recommendVisit = getTravelRecommendation(TravelAdviceResponse.steps.Select(k => k.climate), minTemp, maxTemp);
           TravelAdviceResponse.travelAdvice = await recommendVisit;

           applicationLegResult.TravelPlans = TravelAdviceResponse;
           applicationLegResult.ServiceSuccess = true;
           return applicationLegResult;
        }

        private  WeatherForecast getWeatherFromForecast(Dictionary<string, OpenWeather> tableOfForecasts , string geoHashedKey)
        {
            OpenWeather climate;
            bool geoHashExists = tableOfForecasts.TryGetValue(geoHashedKey, out climate);

            if (geoHashExists)
            {
                return new WeatherForecast
                {
                    Date = DateTime.Now,
                    TemperatureC = climate.main.temp,
                    Summary = climate.weather[0].description
                };
            }
            else
            {
               return new WeatherForecast
                {
                    Date = DateTime.Now,
                    TemperatureC = -999,
                    Summary = "No weather information found for this location"
                };
            }
        }
        
    }
    
  
}