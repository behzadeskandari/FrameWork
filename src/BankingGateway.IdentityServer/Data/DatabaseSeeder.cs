#nullable enable

using BankingGateway.IdentityServer.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace BankingGateway.IdentityServer.Data;

/// <summary>
/// Seeds the database with initial roles, admin user, and OpenIddict applications.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var appManager = services.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = services.GetRequiredService<IOpenIddictScopeManager>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        // 1. Apply pending EF migrations
        await db.Database.MigrateAsync();

        // 2. Seed roles
        foreach (var roleName in new[] { "Admin", "Writer", "Auditor", "User" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    IsSystemRole = true,
                    Description = $"System {roleName} role"
                });

                if (!result.Succeeded)
                    logger.LogError("Failed to create role {Role}: {Errors}", roleName,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // 3. Seed initial admin user
        var adminEmail = configuration["Seed:Admin:Email"] ?? "admin@bankinggateway.local";
        var adminPassword = configuration["Seed:Admin:Password"] ?? throw new InvalidOperationException("Seed:Admin:Password must be configured.");
        var adminFirstName = configuration["Seed:Admin:FirstName"] ?? "System";
        var adminLastName = configuration["Seed:Admin:LastName"] ?? "Administrator";

        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = adminFirstName,
                LastName = adminLastName,
                IsActive = true,
                MustChangePassword = true,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Admin user '{Email}' created.", adminEmail);
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        // 4. Seed OpenIddict scope
        if (await scopeManager.FindByNameAsync("banking_api") is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "banking_api",
                DisplayName = "Banking API",
                Resources = { "banking-gateway" }
            });
        }

        // 4a. banking-gateway (Authorization Code + PKCE)
        var gatewayClientId = "banking-gateway";
        if (await appManager.FindByClientIdAsync(gatewayClientId) is null)
        {
            var gatewayRedirectUri = configuration["Seed:Clients:GatewayRedirectUri"] ?? "https://localhost:5000/signin-oidc";
            var gatewayPostLogoutUri = configuration["Seed:Clients:GatewayPostLogoutUri"] ?? "https://localhost:5000/signout-callback-oidc";
            var gatewaySecret = configuration["OpenIddict:GatewayClientSecret"] ?? throw new InvalidOperationException("OpenIddict:GatewayClientSecret must be configured.");

            await appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = gatewayClientId,
                ClientSecret = gatewaySecret,
                DisplayName = "Banking Gateway",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Logout,
                    OpenIddictConstants.Permissions.Endpoints.Introspection,
                    OpenIddictConstants.Permissions.Endpoints.Revocation,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "banking_api"
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                },
                RedirectUris = { new Uri(gatewayRedirectUri) },
                PostLogoutRedirectUris = { new Uri(gatewayPostLogoutUri) }
            });
        }

        // 4b. banking-service-m2m (Client Credentials)
        var m2mClientId = "banking-service-m2m";
        if (await appManager.FindByClientIdAsync(m2mClientId) is null)
        {
            await appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = m2mClientId,
                ClientSecret = Guid.NewGuid().ToString("N"),
                DisplayName = "Banking Service M2M",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Introspection,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "banking_api"
                }
            });
        }

        // 4c. banking-spa (Public client — Authorization Code + PKCE, no secret)
        var spaClientId = "banking-spa";
        if (await appManager.FindByClientIdAsync(spaClientId) is null)
        {
            var spaRedirectUri = configuration["Seed:Clients:SpaRedirectUri"] ?? "https://localhost:3000/callback";

            await appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = spaClientId,
                DisplayName = "Banking SPA",
                ClientType = OpenIddictConstants.ClientTypes.Public,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Logout,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "banking_api"
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                },
                RedirectUris = { new Uri(spaRedirectUri) }
            });
        }
    }
}
