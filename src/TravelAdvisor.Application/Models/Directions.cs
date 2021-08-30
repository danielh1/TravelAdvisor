using System.Collections.Generic;

namespace TravelAdvisor.Application.Models
{
    public class Directions
    {
        public string Start { get; set; }
        public string End { get; set; }
        public string TotalDuration { get; set; }
        public string TotalDistance { get; set; }
        public List<string> Route { get; set; }
        public string? MapURL { get; set; }
    }
}