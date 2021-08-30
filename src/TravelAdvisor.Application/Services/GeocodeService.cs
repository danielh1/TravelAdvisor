using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using GoogleApi;
using GoogleApi.Entities.Maps.Geocoding.Address.Request;
using Microsoft.Extensions.Configuration;
using TravelAdvisor.Application.Interfaces;
using TravelAdvisor.Application.Models;


namespace TravelAdvisor.Application.Services
{
    public class GeocodeService : IGeocodeService
    {
        private readonly IConfiguration _configuration;

        public GeocodeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Geocode> GeocodeAddressAsync(string address)
        {
            try
            {
                var request = new AddressGeocodeRequest
                {
                    Address = address,
                    Key = _configuration["GoogleApiKey"]
                };
                var response = await GoogleMaps.AddressGeocode.QueryAsync(request);
                var results = response.Results.FirstOrDefault();
                var geocode = new Geocode
                {
                    FormattedAddress = results.FormattedAddress,
                    PlaceId = results.PlaceId,
                    PartialMatch = results.PartialMatch,
                    Geometry = new Geometry
                    {
                        Latitude = results.Geometry.Location.Latitude,
                        Longitude = results.Geometry.Location.Longitude
                    }
                };
                return geocode;
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
    }
}