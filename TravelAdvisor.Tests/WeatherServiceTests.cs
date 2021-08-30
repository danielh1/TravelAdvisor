using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TravelAdvisor.Application.Services;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TravelAdvisor.Application.Models;
using Xunit.Abstractions;

namespace TravelAdvisor.Tests
{
    public class WeatherServiceTests : BaseTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        
        private IHttpClientFactory _httpClientFactory;
        private ServiceCollection _services;
        private ServiceProvider _provider;
        
        
        private readonly WeatherService _sut;
        private Mock<IHttpClientFactory> _httpClientFactoryMock = new Mock<IHttpClientFactory>();
       // private Mock<IConfiguration> _configurationMock = new Mock<IConfiguration>();

        public WeatherServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            
            _services = new ServiceCollection();
            _services.AddHttpClient();
            _provider = _services.BuildServiceProvider();
            
           // _sut = new WeatherService(Configuration, _httpClientFactoryMock);
        }

        [Theory]
        [InlineData(32.7940687, 34.9930623)]
        [InlineData(32.791463, 35.0003694)]
        [InlineData(43.6557259, -79.38373369999999)]
        [InlineData(45.2083635, -74.3482772)]
        public async Task GetWeather_should_return_one_item(double lat, double lng)
        {
            _httpClientFactory = (IHttpClientFactory)_provider.GetService(typeof(IHttpClientFactory));
            var _sut = new WeatherService(Configuration, _httpClientFactory);

            var result = await _sut.GetWeather(lat, lng);
            result.Should().NotBeNull();
       

            var TemperatureC = result.main.temp;
            var Summary = result.weather[0].description;
            
            _testOutputHelper.WriteLine("Temperature : {0}, Summary : {1}, for [coordinatates : {2} , {3}]", TemperatureC, Summary , result.coord.lat, result.coord.lon );
            result.coord.lat.Should().BeApproximately(lat, 0.0001);
            result.coord.lon.Should().BeApproximately(lng, 0.0001);
            // result.weather.First().
            //
            // actors.Cast.Should().Contain(actor => actor.Name == "Emilia Clarke");

        }
        
        [Fact]
        public async Task GetForecasts_should_return_valid_temperatures() {
            
            IReadOnlyList<Coordinates> coordinatesData = CoordinateList.AsReadOnly();
            
            _httpClientFactory = (IHttpClientFactory)_provider.GetService(typeof(IHttpClientFactory));
            var _sut = new WeatherService(Configuration, _httpClientFactory);
            
            var result = await _sut.GetForecasts(coordinatesData);
            result.Should().NotBeNull();
            
            var climates = result.Select(c =>
            {
                var weatherConditions = new WeatherForecast
                {
                    Date = DateTime.Now,
                    TemperatureC = c.main.temp,
                    Summary = c.weather[0].description
                };
                var clmts = new Climate(weatherConditions.TemperatureC, weatherConditions.Summary);
                return clmts;
            });
            string weatherForecasts = JsonConvert.SerializeObject(climates, Formatting.Indented);
            _testOutputHelper.WriteLine(weatherForecasts);
            result.Count.Should().Be(climates.Count());
        }
        
        
        
    }
}