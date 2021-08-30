using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TravelAdvisor.Application.Exceptions;
using TravelAdvisor.Application.Interfaces;
using TravelAdvisor.Application.Models;

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

        

        [Route("/api/directions")]
        [HttpGet]
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
        
        [Route("/api/tour")]
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
        
        
        [Route("/api/travel")]
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