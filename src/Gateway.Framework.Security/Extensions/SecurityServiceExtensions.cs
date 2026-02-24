using Gateway.Framework.Security.Authentication;
using Gateway.Framework.Security.Authorization;
using Gateway.Framework.Shared.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace Gateway.Framework.Security.Extensions;

/// <summary>
/// Extension methods for registering security services.
/// </summary>
public static class SecurityServiceExtensions
{
    /// <summary>
    /// Adds all security services including JWT auth, authorization, and rate limiting.
    /// </summary>
    public static IServiceCollection AddGatewaySecurity(this IServiceCollection services, SecuritySettings settings)
    {
        services.AddGatewayJwtAuthentication(settings.Jwt);
        services.AddGatewayAuthorization();

        if (settings.RateLimit.Enabled)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;
                options.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.PermitLimit = settings.RateLimit.PermitLimit;
                    opt.Window = TimeSpan.FromSeconds(settings.RateLimit.WindowSeconds);
                    opt.QueueLimit = settings.RateLimit.QueueLimit;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });
            });
        }

        return services;
    }

    /// <summary>
    /// Configures the security middleware pipeline.
    /// </summary>
    public static IApplicationBuilder UseGatewaySecurity(this IApplicationBuilder app, SecuritySettings settings)
    {
        if (settings.EnableHttpsRedirection)
        {
            app.UseHttpsRedirection();
        }

        if (settings.EnableHsts)
        {
            app.UseHsts();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
