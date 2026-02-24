using Gateway.Framework.Shared.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gateway.Framework.Security.Authentication;

/// <summary>
/// JWT Bearer authentication configuration.
/// </summary>
public static class JwtAuthenticationSetup
{
    /// <summary>
    /// Configures JWT Bearer authentication with the provided settings.
    /// </summary>
    public static IServiceCollection AddGatewayJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;
            
            if (!string.IsNullOrEmpty(jwtSettings.Authority))
            {
                options.Authority = jwtSettings.Authority;
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = jwtSettings.ValidateIssuer,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = jwtSettings.ValidateAudience,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = jwtSettings.ValidateLifetime,
                ValidateIssuerSigningKey = !string.IsNullOrEmpty(jwtSettings.SecretKey),
                IssuerSigningKey = string.IsNullOrEmpty(jwtSettings.SecretKey)
                    ? null
                    : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
