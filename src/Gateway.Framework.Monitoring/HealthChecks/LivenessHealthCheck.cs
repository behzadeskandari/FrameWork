using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gateway.Framework.Monitoring.HealthChecks;

/// <summary>
/// Liveness probe - indicates the application is running.
/// </summary>
public class LivenessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("Application is alive."));
    }
}
