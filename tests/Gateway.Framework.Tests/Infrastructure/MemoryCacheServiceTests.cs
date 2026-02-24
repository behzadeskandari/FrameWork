using Gateway.Framework.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Gateway.Framework.Tests.Infrastructure;

public class MemoryCacheServiceTests
{
    private readonly MemoryCacheService _cacheService;

    public MemoryCacheServiceTests()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new MemoryCacheService(cache);
    }

    [Fact]
    public async Task SetAndGet_ShouldReturnValue()
    {
        await _cacheService.SetAsync("key1", "value1");
        var result = await _cacheService.GetAsync<string>("key1");
        
        Assert.Equal("value1", result);
    }

    [Fact]
    public async Task Get_NonExistentKey_ShouldReturnDefault()
    {
        var result = await _cacheService.GetAsync<string>("nonexistent");
        
        Assert.Null(result);
    }

    [Fact]
    public async Task Remove_ShouldDeleteKey()
    {
        await _cacheService.SetAsync("key2", "value2");
        await _cacheService.RemoveAsync("key2");
        var result = await _cacheService.GetAsync<string>("key2");
        
        Assert.Null(result);
    }

    [Fact]
    public async Task Exists_ShouldReturnTrue_ForExistingKey()
    {
        await _cacheService.SetAsync("key3", "value3");
        var exists = await _cacheService.ExistsAsync("key3");
        
        Assert.True(exists);
    }

    [Fact]
    public async Task Exists_ShouldReturnFalse_ForMissingKey()
    {
        var exists = await _cacheService.ExistsAsync("missing");
        
        Assert.False(exists);
    }
}
