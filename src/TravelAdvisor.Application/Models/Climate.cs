namespace TravelAdvisor.Application.Models
{
    public class Climate
    {
        public Climate(double celsiusTemp, string description)
        {
            this.celsiusTemp = celsiusTemp;
            this.description = description;
        }

        public double celsiusTemp { get; set; }
        public string description { get; set; }
    }
}