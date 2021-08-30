using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TravelAdvisor.Application.Exceptions;
using TravelAdvisor.Application.Interfaces;
using TravelAdvisor.Application.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace TravelAdvisor.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class DirectionsController : ControllerBase
    {
        private readonly IDirectionService _directionService;
        private readonly ITouringService _touringService;
        
        public DirectionsController(IDirectionService directionService, ITouringService touringService)
        {
            _directionService = directionService;
            _touringService = touringService;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="map"></param>
        /// <returns></returns>

        [Route("/api/directions")]
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Get([FromQuery] string origin, string destination ,bool map)
        {
            if (string.IsNullOrEmpty(origin)|string.IsNullOrEmpty(destination))
            {
                return BadRequest();
            }
            try
            {
                var Response = await _directionService.GetDirectionsAsync(origin, destination, map);
                if (Response is null)
                {
                    return StatusCode(404);
                }
                return Ok(Response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
        
        /// <summary>
        /// Retrieve TravelAdvice by performing PARALLEL http requests for geographic coordinates to OpenWeatherMap API.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>"Yes/No"</c> determination for undertaking the excursion will be governed by whether the temperature of the end_location in each step along
        /// the travel directions is within Min / Max range.
        /// </para>
        /// <para>
        ///  NOTE: Some temperature values for the "climate" attribute may have a value of -999 as the is presently an
        ///  unresolved race condition to access the Redis Keys
        /// </para>
        /// </remarks>
        /// <param name="origin">The start location of excursion</param>
        /// <param name="destination">The end location of excursion</param>
        /// <param name="minimumTemperature">The minimum acceptable temperature in celsuis for a step</param>
        /// <param name="maxTemperature">The maximum acceptable temperature in celsuis for a step</param>
        /// <returns></returns>
        /// <exception cref="ApplicationValidationException"></exception>
        [Route("/api/parallel")]
        [HttpGet]
        public async Task<ActionResult<ApplicationMessage<Leg>>> GetTour([FromQuery] string origin, string destination, double minimumTemperature = 20.0, double maxTemperature = 30.0)
        {
            
            if (string.IsNullOrEmpty(origin)|string.IsNullOrEmpty(destination))
            {
                throw new ApplicationValidationException($"Origin and Destination location cannot be empty");
            }
            
            if (string.IsNullOrEmpty(origin)|string.IsNullOrEmpty(destination))
            {
                return BadRequest();
            }
            try
            {
                var Response = await _touringService.GetDirectionsFastAsync(origin, destination, minimumTemperature, maxTemperature);
                if (Response is null)
                {
                    return StatusCode(404);
                }
                return Ok(Response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
        
        /// <summary>
        /// Retrieve TravelAdvice by performing SEQUENTIAL http requests for geographic coordinates to OpenWeatherMap API.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>"Yes/No"</c> determination for undertaking the excursion will be governed by whether the temperature of the end_location in each step along
        /// the travel directions is within Min / Max range.
        /// </para>
        /// </remarks>
        /// <param name="origin">The start location of excursion</param>
        /// <param name="destination">The end location of excursion</param>
        /// <param name="minimumTemperature">The minimum acceptable temperature in celsuis for a step</param>
        /// <param name="maxTemperature">The maximum acceptable temperature in celsuis for a step</param>
        /// <returns></returns>
        /// <exception cref="ApplicationValidationException"></exception>
        [Route("/api/sequential")]
        [HttpGet]
        public async Task<ActionResult<Leg>> GetTravel([FromQuery] string origin, string destination, double minimumTemperature = 20.0, double maxTemperature = 30.0)
        {
            
            if (string.IsNullOrEmpty(origin)|string.IsNullOrEmpty(destination))
            {
                throw new ApplicationValidationException($"Origin and Destination location cannot be empty");
            }
            try
            {
                var Response = await _touringService.GetDirectionsSlowAsync(origin, destination); //, minimumTemperature, maxTemperature);
                if (Response is null)
                {
                    return StatusCode(404);
                }
                return Ok(Response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
        
    }
}