using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TravelAdvisor.Application.Interfaces;


namespace TravelAdvisor.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class GeocodeController : ControllerBase
    {
        private readonly IGeocodeService _GeocodeService;
        public GeocodeController(IGeocodeService geocodeService)
        {
            _GeocodeService = geocodeService;
        }

        [Route("/api/geocode")]
        [HttpPost]
        public async Task<IActionResult> Get([FromBody] string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return BadRequest();
            }
            try
            {
                return Ok(await _GeocodeService.GeocodeAddressAsync(address));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
    }
}