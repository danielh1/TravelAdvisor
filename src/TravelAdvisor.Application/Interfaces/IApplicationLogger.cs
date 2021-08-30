namespace TravelAdvisor.Application.Interfaces
{
    public interface IApplicatonLogger<T>
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(string message, params object[] args);
        
        void LogDebug (string message, params object[] args);
    }
}