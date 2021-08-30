using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using TravelAdvisor.API;
using TravelAdvisor.API.Controllers;
using TravelAdvisor.Application.Interfaces;
using FluentAssertions;
using Newtonsoft.Json;
using TravelAdvisor.Application.Models;
using Xunit;

namespace TravelAdvisor.Tests
{
   
    
    public class DirectionsControllerTest
    {
        private readonly HttpClient _client;

        public DirectionsControllerTest()
        {
            var appFactory = new WebApplicationFactory<Startup>();
            _client = appFactory.CreateClient();

        }
        
        [Fact]
        public async Task GetTour_should_return_correct_number_of_steps()
        {
            // Arrange
            
            // private readonly IDirectionService _directionService;
            // private readonly ITouringService _touringService;

            // var directionService = A.Fake<IDirectionService>();
            // var touringService = A.Fake<ITouringService>();
            // A.CallTo()
            // var controller = new DirectionsController(directionService,  touringService);

            var response =
                await _client.GetAsync(
                    "/api/tour?origin=Tel%20Aviv&destination=Netanya&minimumTemperature=20&maxTemperature=30");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var forecast = JsonConvert.DeserializeObject<ApplicationMessage<Leg>>(await response.Content.ReadAsStringAsync());
            forecast.TravelPlans.steps.Should().HaveCount(20);
        }
        
        [Fact]
        public async Task GetTour_should_recommend_travel_between_TelAviv_and_Netanya()
        {
            var response =
                await _client.GetAsync(
                    "/api/tour?origin=Tel%20Aviv&destination=Netanya&minimumTemperature=20&maxTemperature=40");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var forecast = JsonConvert.DeserializeObject<ApplicationMessage<Leg>>(await response.Content.ReadAsStringAsync());
            forecast.TravelPlans.travelAdvice.Should().BeEquivalentTo("Yes");
        }
        
        

    }
}