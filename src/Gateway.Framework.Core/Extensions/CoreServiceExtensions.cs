using FluentValidation;
using Gateway.Framework.Core.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Framework.Core.Extensions;

/// <summary>
/// Extension methods for registering core framework services.
/// </summary>
public static class CoreServiceExtensions
{
    /// <summary>
    /// Adds core framework services including validation and filters.
    /// </summary>
    public static IServiceCollection AddGatewayCore(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationActionFilter>();
        });

        return services;
    }

    /// <summary>
    /// Adds FluentValidation validators from the specified assemblies.
    /// </summary>
    public static IServiceCollection AddGatewayValidation(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
    {
        services.AddValidatorsFromAssemblies(assemblies);
        return services;
    }
}
