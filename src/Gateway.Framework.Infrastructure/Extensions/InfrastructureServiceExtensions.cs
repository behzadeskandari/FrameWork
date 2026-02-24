using Gateway.Framework.Core.Interfaces;
using Gateway.Framework.Infrastructure.Caching;
using Gateway.Framework.Shared.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Gateway.Framework.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Adds infrastructure services including caching and feature management.
    /// </summary>
    public static IServiceCollection AddGatewayInfrastructure(this IServiceCollection services, CacheSettings cacheSettings)
    {
        // Caching
        if (cacheSettings.UseDistributedCache && !string.IsNullOrEmpty(cacheSettings.RedisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheSettings.RedisConnectionString;
                options.InstanceName = cacheSettings.InstanceName;
            });
            services.AddSingleton<ICacheService>(sp =>
                new DistributedCacheService(
                    sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>(),
                    TimeSpan.FromMinutes(cacheSettings.DefaultExpirationMinutes)));
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService>(sp =>
                new MemoryCacheService(
                    sp.GetRequiredService<IMemoryCache>(),
                    TimeSpan.FromMinutes(cacheSettings.DefaultExpirationMinutes)));
        }

        // Feature Flags
        services.AddFeatureManagement();

        return services;
    }
}
