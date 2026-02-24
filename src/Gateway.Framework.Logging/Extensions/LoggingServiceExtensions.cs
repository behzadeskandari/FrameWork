using Gateway.Framework.Core.Interfaces;
using Gateway.Framework.Logging.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Framework.Logging.Extensions;

/// <summary>
/// Extension methods for registering logging services.
/// </summary>
public static class LoggingServiceExtensions
{
    /// <summary>
    /// Adds gateway logging services including audit logging.
    /// </summary>
    public static IServiceCollection AddGatewayLoggingServices(this IServiceCollection services)
    {
        services.AddSingleton<IAuditLogger, AuditLogger>();
        return services;
    }
}
