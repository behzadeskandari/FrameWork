using System.Text.Json;
using Gateway.Framework.Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Gateway.Framework.Infrastructure.Caching;

/// <summary>
/// Distributed (Redis-ready) cache implementation.
/// </summary>
public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _defaultExpiration;

    public DistributedCacheService(IDistributedCache cache, TimeSpan? defaultExpiration = null)
    {
        _cache = cache;
        _defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(30);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var data = await _cache.GetStringAsync(key, cancellationToken);
        return data == null ? default : JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
        };
        var data = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, data, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var data = await _cache.GetAsync(key, cancellationToken);
        return data != null;
    }
}
