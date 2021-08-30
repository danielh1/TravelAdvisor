using System;
using System.Threading.Tasks;

namespace TravelAdvisor.Application.Interfaces
{
    public interface ICacheService
    {
        Task<T> GetOrSet<T>(string key, Func<Task<T>> factory, TimeSpan cacheExpiry);
    }
}