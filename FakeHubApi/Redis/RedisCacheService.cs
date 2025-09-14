using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace FakeHubApi.Redis;

public interface IRedisCacheService
{
    Task SetCacheValueAsync<T>(string key, T value);
    Task<T> GetCacheValueAsync<T>(string key);
    Task RemoveCacheValueAsync(string key);
}

public class RedisCacheService(IDistributedCache _cache) : IRedisCacheService
{
    public async Task SetCacheValueAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });
    }

    public async Task<T> GetCacheValueAsync<T>(string key)
    {
        var json = await _cache.GetStringAsync(key);
        return json is null ? default : JsonSerializer.Deserialize<T>(json);
    }
    
    public async Task RemoveCacheValueAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }
}