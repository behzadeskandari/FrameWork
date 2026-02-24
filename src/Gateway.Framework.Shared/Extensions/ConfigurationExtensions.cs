using Gateway.Framework.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Framework.Shared.Extensions;

/// <summary>
/// Extension methods for binding configuration.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Binds and registers GatewaySettings from configuration.
    /// </summary>
    public static IServiceCollection AddGatewayConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GatewaySettings>(configuration.GetSection(GatewaySettings.SectionName));
        services.Configure<SecuritySettings>(configuration.GetSection($"{GatewaySettings.SectionName}:Security"));
        services.Configure<JwtSettings>(configuration.GetSection($"{GatewaySettings.SectionName}:Security:Jwt"));
        services.Configure<RateLimitSettings>(configuration.GetSection($"{GatewaySettings.SectionName}:Security:RateLimit"));
        services.Configure<CacheSettings>(configuration.GetSection($"{GatewaySettings.SectionName}:Cache"));
        services.Configure<ResilienceSettings>(configuration.GetSection($"{GatewaySettings.SectionName}:Resilience"));
        return services;
    }
}
