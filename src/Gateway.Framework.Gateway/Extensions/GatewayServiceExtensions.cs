using Gateway.Framework.Gateway.Transforms;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Transforms;

namespace Gateway.Framework.Gateway.Extensions;

/// <summary>
/// Extension methods for registering YARP reverse proxy services.
/// </summary>
public static class GatewayServiceExtensions
{
    /// <summary>
    /// Adds YARP reverse proxy services with configuration from appsettings.
    /// </summary>
    public static IServiceCollection AddGatewayProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddTransforms(builderContext =>
            {
                // Add correlation ID to all proxied requests
                builderContext.AddRequestTransform(transformContext =>
                {
                    var correlationId = transformContext.HttpContext.Items["CorrelationId"]?.ToString()
                        ?? Guid.NewGuid().ToString("D");

                    transformContext.ProxyRequest.Headers.Remove("X-Correlation-ID");
                    transformContext.ProxyRequest.Headers.Add("X-Correlation-ID", correlationId);

                    return ValueTask.CompletedTask;
                });
            });

        return services;
    }

    /// <summary>
    /// Maps the YARP reverse proxy endpoints.
    /// </summary>
    public static WebApplication MapGatewayProxy(this WebApplication app)
    {
        app.MapReverseProxy();
        return app;
    }
}
