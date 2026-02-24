using Gateway.Framework.Monitoring.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Gateway.Framework.Monitoring.Extensions;

/// <summary>
/// Extension methods for registering monitoring and observability services.
/// </summary>
public static class MonitoringServiceExtensions
{
    /// <summary>
    /// Adds health checks, OpenTelemetry tracing and metrics.
    /// </summary>
    public static IServiceCollection AddGatewayMonitoring(this IServiceCollection services, string serviceName = "BankingGateway")
    {
        // Health Checks
        services.AddHealthChecks()
            .AddCheck<LivenessHealthCheck>("liveness", tags: new[] { "liveness" })
            .AddCheck<ReadinessHealthCheck>("readiness", tags: new[] { "readiness" });

        // OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();
            });

        return services;
    }

    /// <summary>
    /// Maps health check endpoints and Prometheus metrics.
    /// </summary>
    public static IEndpointRouteBuilder MapGatewayMonitoring(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("liveness"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("readiness"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints.MapPrometheusScrapingEndpoint("/metrics");

        return endpoints;
    }
}
