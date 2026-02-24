using BankingGateway.Core.Configuration;
using BankingGateway.Core.Interfaces;
using BankingGateway.Infrastructure.Caching;
using BankingGateway.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankingGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Correlation ID
        services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

        // Audit logging
        services.AddSingleton<IAuditLogger, AuditLogger>();

        // Caching - choose based on config
        var redisSettings = configuration.GetSection(RedisSettings.SectionName).Get<RedisSettings>();
        if (redisSettings is not null && !string.IsNullOrWhiteSpace(redisSettings.ConnectionString)
            && redisSettings.ConnectionString != "localhost:6379")
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisSettings.ConnectionString;
                options.InstanceName = redisSettings.InstanceName;
            });
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }

        // Configuration bindings
        services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.SectionName));
        services.Configure<ResilienceSettings>(configuration.GetSection(ResilienceSettings.SectionName));
        services.Configure<RateLimitSettings>(configuration.GetSection(RateLimitSettings.SectionName));
        services.Configure<SecurityHeadersSettings>(configuration.GetSection(SecurityHeadersSettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        return services;
    }
}
