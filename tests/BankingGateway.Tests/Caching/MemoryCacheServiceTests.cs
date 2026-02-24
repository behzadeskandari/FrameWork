using BankingGateway.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace BankingGateway.Tests.Caching;

public class MemoryCacheServiceTests
{
    private readonly MemoryCacheService _sut;
    private readonly IMemoryCache _cache;

    public MemoryCacheServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        var logger = Mock.Of<ILogger<MemoryCacheService>>();
        _sut = new MemoryCacheService(_cache, logger);
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_Should_RoundTrip()
    {
        await _sut.SetAsync("key1", "value1", TimeSpan.FromMinutes(1));

        var result = await _sut.GetAsync<string>("key1");

        result.Should().Be("value1");
    }

    [Fact]
    public async Task GetAsync_Should_Return_Default_When_Key_Missing()
    {
        var result = await _sut.GetAsync<string>("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_Should_Remove_Key()
    {
        await _sut.SetAsync("key2", "value2");

        await _sut.RemoveAsync("key2");

        var result = await _sut.GetAsync<string>("key2");
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_True_When_Key_Exists()
    {
        await _sut.SetAsync("key3", 42);

        var exists = await _sut.ExistsAsync("key3");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_False_When_Key_Missing()
    {
        var exists = await _sut.ExistsAsync("missing");

        exists.Should().BeFalse();
    }
}
