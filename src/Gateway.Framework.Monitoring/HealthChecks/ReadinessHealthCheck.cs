using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gateway.Framework.Monitoring.HealthChecks;

/// <summary>
/// Readiness probe - indicates the application is ready to serve traffic.
/// </summary>
public class ReadinessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // This can be extended to check downstream dependencies
        return Task.FromResult(HealthCheckResult.Healthy("Application is ready."));
    }
}
