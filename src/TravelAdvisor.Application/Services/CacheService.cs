using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StackExchange.Redis;
using TravelAdvisor.Application.Interfaces;

namespace TravelAdvisor.Application.Services
{
    public class CacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _muxer;
        private readonly IDatabase _conn;
        private readonly IMemoryCache _memCache;

        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1);

        public CacheService(IConnectionMultiplexer muxer, IMemoryCache memCache)
        {
            _muxer = muxer;
            _conn = _muxer.GetDatabase();
            _memCache = memCache;
        }

        public async Task<T> GetOrSet<T>(string key, Func<Task<T>> factory, TimeSpan cacheExpiry)
        {
            // await _cacheLock.WaitAsync();
            var value = await _memCache.GetOrCreateAsync<T>(key, entry =>
            {
                entry.AbsoluteExpiration = DateTime.UtcNow.Add(cacheExpiry);
                return GetFromRedis(key, factory, cacheExpiry);
            });
            // _cacheLock.Release();
            return value;
            
        }

        private async Task<T> GetFromRedis<T>(string key, Func<Task<T>> factory, TimeSpan cacheExpiry)
        {
            
            try
            {
                var value = await _conn.StringGetAsync(key);

                if (value.HasValue)
                {
                    
                    try
                    {
                        return JsonConvert.DeserializeObject<T>(value);
                    }
                    catch (Exception)
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                }

                var item = await factory.Invoke();
                if (item != null)
                {
                    var serializedValue = JsonConvert.SerializeObject(item);
                    await _conn.StringSetAsync(key, serializedValue, cacheExpiry, When.Always, CommandFlags.None);

                    return item;
                }

                return default(T);
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}