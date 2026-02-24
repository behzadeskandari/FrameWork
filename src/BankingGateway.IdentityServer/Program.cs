#nullable enable

using BankingGateway.IdentityServer.Data;
using BankingGateway.IdentityServer.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Serilog;
using System.Security.Cryptography.X509Certificates;

// ── Serilog bootstrap ──────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/identityserver-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ─────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "BankingGateway.IdentityServer")
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File("logs/identityserver-.log", rollingInterval: RollingInterval.Day));

    // ── Entity Framework Core ────────────────────────────────────────
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityServer"),
            sql =>
            {
                sql.MigrationsAssembly("BankingGateway.IdentityServer");
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                sql.CommandTimeout(60);
            });

        // Required for OpenIddict
        options.UseOpenIddict<Guid>();
    });

    // ── ASP.NET Core Identity ────────────────────────────────────────
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password policy — banking-grade
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredUniqueChars = 6;

            // Lockout — banking-grade
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // set true in production
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // ── OpenIddict ───────────────────────────────────────────────────
    builder.Services.AddOpenIddict()
        .AddCore(options =>
        {
            options.UseEntityFrameworkCore()
                   .UseDbContext<ApplicationDbContext>()
                   .ReplaceDefaultEntities<Guid>();
        })
        .AddServer(options =>
        {
            // Issuer
            options.SetIssuer(new Uri(builder.Configuration["OpenIddict:Issuer"]!));

            // Endpoints
            options.SetAuthorizationEndpointUris("/connect/authorize")
                   .SetTokenEndpointUris("/connect/token")
                   .SetIntrospectionEndpointUris("/connect/introspect")
                   .SetRevocationEndpointUris("/connect/revoke")
                   .SetUserinfoEndpointUris("/connect/userinfo")
                   .SetLogoutEndpointUris("/connect/logout");

            // Flows
            options.AllowAuthorizationCodeFlow()
                   .RequireProofKeyForCodeExchange(); // PKCE — mandatory
            options.AllowClientCredentialsFlow();
            options.AllowRefreshTokenFlow();

            // Token lifetimes
            options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));
            options.SetRefreshTokenLifetime(TimeSpan.FromDays(1));
            options.SetAuthorizationCodeLifetime(TimeSpan.FromMinutes(5));

            // Scopes
            options.RegisterScopes(
                OpenIddictConstants.Scopes.OpenId,
                OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Scopes.Roles,
                OpenIddictConstants.Scopes.OfflineAccess,
                "banking_api"
            );

            // Signing & encryption
            if (builder.Environment.IsDevelopment())
            {
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();
            }
            else
            {
                var signingThumbprint = builder.Configuration["OpenIddict:Certificates:Signing:Thumbprint"]!;
                var encryptionThumbprint = builder.Configuration["OpenIddict:Certificates:Encryption:Thumbprint"]!;
                options.AddSigningCertificate(LoadCertificateFromStore(signingThumbprint))
                       .AddEncryptionCertificate(LoadCertificateFromStore(encryptionThumbprint));
            }

            // Reference tokens — persisted in DB, revocable server-side
            options.UseReferenceAccessTokens()
                   .UseReferenceRefreshTokens();

            options.UseAspNetCore()
                   .EnableAuthorizationEndpointPassthrough()
                   .EnableTokenEndpointPassthrough()
                   .EnableUserinfoEndpointPassthrough()
                   .EnableLogoutEndpointPassthrough()
                   .EnableStatusCodePagesIntegration()
                   .DisableTransportSecurityRequirement(); // only in dev; remove in prod
        })
        .AddValidation(options =>
        {
            options.UseLocalServer(); // validate tokens using the local server
            options.UseAspNetCore();
        });

    // ── MVC + Razor Pages ────────────────────────────────────────────
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    // ── Authorization policies ───────────────────────────────────────
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
        options.AddPolicy("BankingApi", p => p.RequireAuthenticatedUser().RequireClaim("scope", "banking_api"));
        options.AddPolicy("ReadAccess", p => p.RequireAuthenticatedUser());
        options.AddPolicy("WriteAccess", p => p.RequireRole("Admin", "Writer"));
        options.AddPolicy("AuditAccess", p => p.RequireRole("Admin", "Auditor"));
    });

    // ════════════════════════════════════════════════════════════════
    var app = builder.Build();

    // ── Database seeding ─────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
    }

    // ── Middleware pipeline ──────────────────────────────────────────
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapRazorPages();

    // ── Serilog request logging ──────────────────────────────────────
    app.UseSerilogRequestLogging();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Identity server terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// ── Certificate helper ───────────────────────────────────────────────
static X509Certificate2 LoadCertificateFromStore(string thumbprint)
{
    using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
    store.Open(OpenFlags.ReadOnly);
    var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: true);
    if (certs.Count == 0)
        throw new InvalidOperationException($"Certificate with thumbprint '{thumbprint}' not found.");
    return certs[0];
}
