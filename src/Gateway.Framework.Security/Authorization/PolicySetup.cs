using Gateway.Framework.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Framework.Security.Authorization;

/// <summary>
/// Authorization policy configuration.
/// </summary>
public static class PolicySetup
{
    /// <summary>
    /// Configures role-based and policy-based authorization.
    /// </summary>
    public static IServiceCollection AddGatewayAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(GatewayConstants.Policies.AdminPolicy, policy =>
                policy.RequireRole(GatewayConstants.Roles.Admin))
            .AddPolicy(GatewayConstants.Policies.ReadPolicy, policy =>
                policy.RequireAuthenticatedUser())
            .AddPolicy(GatewayConstants.Policies.WritePolicy, policy =>
                policy.RequireRole(GatewayConstants.Roles.Admin, GatewayConstants.Roles.User))
            .AddPolicy(GatewayConstants.Policies.TransactionPolicy, policy =>
                policy.RequireRole(GatewayConstants.Roles.Admin, GatewayConstants.Roles.User)
                       .RequireClaim(GatewayConstants.ClaimTypes.Permission, "transactions"));

        return services;
    }
}
