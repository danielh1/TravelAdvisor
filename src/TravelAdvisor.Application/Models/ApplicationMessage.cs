namespace TravelAdvisor.Application.Models
{
    public class ApplicationMessage<T>
    {
        public T TravelPlans { get; set; }
        public bool ServiceSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}