using System;
using System.Collections.Generic;

namespace TravelAdvisor.Application.Models
{
    public class Trip
    {
        public IList<Route> routes { get; set; }
    }

    public class Route
    {
        public IList<Leg> legs { get; set; }
    }

    public class Leg : TravelInformation
    {
        public string start_address { get; set; }
        public string end_address { get; set; }

        public IList<TripStep> steps { get; set; }
        public string? travelAdvice { get; set; }
    }


    public class TripStep : TravelInformation
    {
        public string html_instructions { get; set; }
        //#nullable enable
        public Climate? climate { get; set; }
    }

    public class TravelInformation
    {
        public Distance distance { get; set; }
        public TimeDuration duration { get; set; }
        public Coordinates start_location { get; set; }
        public Coordinates end_location { get; set; }
    }

    public class Coordinates
    {
        public Coordinates()
        {
            
        }
        public Coordinates(double lat, double lng, String locationHash)
        {
            this.lat = lat;
            this.lng = lng;
            this.locationHash = locationHash;
        }

        public Coordinates(double lat, double lng)
        {
            this.lat = lat;
            this.lng = lng;
            this.locationHash = "";
        }
        public double lat { get; set; }
        public double lng { get; set; }
        
        public String? locationHash { get; set; }
    }

    public class Distance
    {
        public Distance(string text, long value)
        {
            this.text = text;
            this.value = value;
        }

        public string text { get; set; }
        public long value { get; set; }
    }

    public class TimeDuration
    {
        public TimeDuration(string text, long value)
        {
            this.text = text;
            this.value = value;
        }

        public string text { get; set; }
        public long value { get; set; }
    }
}