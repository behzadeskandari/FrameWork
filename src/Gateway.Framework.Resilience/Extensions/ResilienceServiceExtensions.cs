using Gateway.Framework.Resilience.Policies;
using Gateway.Framework.Shared.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace Gateway.Framework.Resilience.Extensions;

/// <summary>
/// Extension methods for registering resilience services.
/// </summary>
public static class ResilienceServiceExtensions
{
    /// <summary>
    /// Adds a resilient HTTP client with retry, circuit breaker, and timeout policies.
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient(
        this IServiceCollection services,
        string clientName,
        ResilienceSettings settings,
        string? baseAddress = null)
    {
        return services.AddHttpClient(clientName, client =>
        {
            if (!string.IsNullOrEmpty(baseAddress))
            {
                client.BaseAddress = new Uri(baseAddress);
            }
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds + 5);
        })
        .AddPolicyHandler((provider, _) =>
        {
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Gateway.Framework.Resilience");
            return ResiliencePolicies.GetRetryPolicy(settings, logger);
        })
        .AddPolicyHandler((provider, _) =>
        {
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Gateway.Framework.Resilience");
            return ResiliencePolicies.GetCircuitBreakerPolicy(settings, logger);
        });
    }
}
