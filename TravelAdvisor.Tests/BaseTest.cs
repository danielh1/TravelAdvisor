using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using TravelAdvisor.Application.Models;

namespace TravelAdvisor.Tests
{
    public abstract class BaseTest
    {
        protected BaseTest()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
        }
        protected IConfiguration Configuration { get; private set; }
        
        
        protected List<Coordinates> CoordinateList = new List<Coordinates>()
        {
            new Coordinates(43.6557259, -79.3837337),
            new Coordinates(43.6618515, -79.3545244),
            new Coordinates(43.66366379999999, -79.3555052),
            new Coordinates(43.7630746, -79.3367385),
            new Coordinates(43.7681346, -79.3259758),
            new Coordinates(43.83811679999999, -79.07197540000001),
            new Coordinates(44.7393893, -75.50060409999999),
            new Coordinates(45.1892867, -74.3833652),
            new Coordinates(45.2083635, -74.3482772),
            new Coordinates(45.44287, -73.6588041),
            new Coordinates(45.4627911, -73.61110649999999),
            new Coordinates(45.4797411, -73.5913317),
            new Coordinates(45.4851586, -73.58294049999999),
            new Coordinates(45.4911521, -73.5774618),
            new Coordinates(45.5018118, -73.56734449999999),
            new Coordinates(45.5017123, -73.5672184),
            new Coordinates(32.7940687, 34.9930623), 
            new Coordinates(32.791463, 35.0003694)
        };
    }
}