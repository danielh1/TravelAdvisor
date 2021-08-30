namespace TravelAdvisor.Application.Models
{
    public class Geocode
    {
        public string FormattedAddress { get; set; }
        public Geometry Geometry { get; set; }
        public bool PartialMatch { get; set; }
        public string PlaceId { get; set; }
    }
    public class Geometry
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}