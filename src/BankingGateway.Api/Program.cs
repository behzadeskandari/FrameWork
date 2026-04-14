

#nullable enable

using BankingGateway.Api.Auth;
using BankingGateway.Api.Middleware;
using BankingGateway.Api.Validation;
using BankingGateway.Core.Configuration;
using BankingGateway.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/gateway-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "BankingGateway")
        .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File("logs/gateway-.log", rollingInterval: RollingInterval.Day));

    // ── Configuration ───────────────────────────────────────────────
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
    var securitySettings = builder.Configuration.GetSection(SecurityHeadersSettings.SectionName).Get<SecurityHeadersSettings>() ?? new SecurityHeadersSettings();
    var rateLimitSettings = builder.Configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>() ?? new RateLimitSettings();

    // ── Infrastructure ──────────────────────────────────────────────
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Controllers + Validation ────────────────────────────────────
    builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ── Authentication ──────────────────────────────────────────────
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIddictClientAspNetCoreDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/api/auth/login";
        options.LogoutPath = "/api/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

    // ── OpenTelemetry ───────────────────────────────────────────────
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("BankingGateway"))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddPrometheusExporter());

    // ── OpenIddict Client + Validation (FINAL VERSION) ──────────────
    builder.Services.AddOpenIddict()
        .AddClient(options =>
        {
            options.AllowAuthorizationCodeFlow();

            // Development keys - REQUIRED for interactive login
            options.AddDevelopmentEncryptionCertificate();
            options.AddDevelopmentSigningCertificate();

            options.UseSystemNetHttp();

            options.UseAspNetCore()
                   .EnableRedirectionEndpointPassthrough()
                   .EnablePostLogoutRedirectionEndpointPassthrough()
                   .DisableTransportSecurityRequirement(); // Only for development

            options.AddRegistration(new OpenIddictClientRegistration
            {
                Issuer = new Uri(builder.Configuration["Jwt:Authority"] ?? "https://localhost:7001"),
                //   Issuer = new Uri(builder.Configuration["OpenIddict:Issuer"] ?? "https://localhost:7001"),
                ClientId = "bankinggateway-api",
                ClientSecret = builder.Configuration["OpenIddict:Clients:BankingGatewayApi:Secret"]
                    ?? "super-strong-secret-change-in-production",

                RedirectUri = new Uri("https://localhost:5001/api/auth/callback"),

                Scopes =
                {
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.OfflineAccess,
                    "banking_api"
                }
            });
        })
        .AddValidation(options =>
        {
            options.SetIssuer(jwtSettings.Authority);
            options.AddAudiences(jwtSettings.Audience);

            options.UseIntrospection()
                   .SetClientId(jwtSettings.Audience)
                   .SetClientSecret(builder.Configuration["OpenIddict:GatewayClientSecret"]
                       ?? throw new InvalidOperationException("OpenIddict:GatewayClientSecret is required."));

            options.UseSystemNetHttp();
            options.UseAspNetCore();
        });

    // ── Authorization ───────────────────────────────────────────────
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
        options.AddPolicy("ReadAccess", p => p.RequireAuthenticatedUser());
        options.AddPolicy("WriteAccess", p => p.RequireRole("Admin", "Writer"));
        options.AddPolicy("BankingApi", p => p.RequireClaim("scope", "banking_api"));
    });

    builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

    // ── Rate Limiting ───────────────────────────────────────────────
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter("fixed", opt =>
        {
            opt.PermitLimit = rateLimitSettings.PermitLimit;
            opt.Window = TimeSpan.FromSeconds(rateLimitSettings.WindowSeconds);
            opt.QueueLimit = rateLimitSettings.QueueLimit;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        });
    });

    // ── YARP Reverse Proxy ──────────────────────────────────────────
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    // ── Health Checks ───────────────────────────────────────────────
    var healthChecksBuilder = builder.Services.AddHealthChecks();

    var redisConfig = builder.Configuration.GetSection(RedisSettings.SectionName).Get<RedisSettings>();
    if (redisConfig is { Enabled: true } && !string.IsNullOrWhiteSpace(redisConfig.ConnectionString))
    {
        healthChecksBuilder.AddRedis(redisConfig.ConnectionString, name: "redis");
    }

    var issuer = builder.Configuration["OpenIddict:Issuer"] ?? "https://localhost:7001";
    healthChecksBuilder.AddUrlGroup(
        new Uri(issuer.TrimEnd('/') + "/.well-known/openid-configuration"),
        name: "identityserver",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded);


    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });

    // ═════════════════════════════════════════════════════════════════
    var app = builder.Build();

    // ── Middleware Pipeline ─────────────────────────────────────────
    app.UseForwardedHeaders();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/swagger"),
        appBuilder => appBuilder.UseMiddleware<SecurityHeadersMiddleware>());


    app.UseRouting();
    app.UseCors(); // If needed

    app.UseMiddleware<IpFilteringMiddleware>();
    app.UseMiddleware<RequestSizeLimitMiddleware>();

    if (!app.Environment.IsDevelopment())
        app.UseHsts();

    app.UseHttpsRedirection();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapReverseProxy();

    app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
    app.MapHealthChecks("/health/ready");

    app.UseOpenTelemetryPrometheusScrapingEndpoint("/metrics");

    app.UseSerilogRequestLogging();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }



//#nullable enable

//using BankingGateway.Api.Auth;
//using BankingGateway.Api.Middleware;
//using BankingGateway.Api.Validation;
//using BankingGateway.Core.Configuration;
//using BankingGateway.Infrastructure;
//using FluentValidation;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.RateLimiting;
//using OpenIddict.Abstractions;
//using OpenIddict.Client;
//using OpenIddict.Client.AspNetCore;
//using OpenIddict.Validation.AspNetCore;
//using OpenTelemetry.Metrics;
//using OpenTelemetry.Resources;
//using OpenTelemetry.Trace;
//using Serilog;
//using System.Threading.RateLimiting;
//using Microsoft.AspNetCore.Authentication;

//Log.Logger = new LoggerConfiguration()
//    .Enrich.FromLogContext()
//    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
//    .WriteTo.File("logs/gateway-.log", rollingInterval: RollingInterval.Day)
//    .CreateBootstrapLogger();

//try
//{
//    var builder = WebApplication.CreateBuilder(args);

//    builder.Host.UseSerilog((ctx, lc) => lc
//        .ReadFrom.Configuration(ctx.Configuration)
//        .Enrich.FromLogContext()
//        .Enrich.WithProperty("Application", "BankingGateway")
//        .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
//        .WriteTo.File("logs/gateway-.log", rollingInterval: RollingInterval.Day));

//    // ── Configuration ───────────────────────────────────────────────
//    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
//    var securitySettings = builder.Configuration.GetSection(SecurityHeadersSettings.SectionName).Get<SecurityHeadersSettings>() ?? new SecurityHeadersSettings();
//    var rateLimitSettings = builder.Configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>() ?? new RateLimitSettings();

//    // ── Infrastructure ──────────────────────────────────────────────
//    builder.Services.AddInfrastructure(builder.Configuration);

//    // ── Controllers + Validation ────────────────────────────────────
//    builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
//    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
//    builder.Services.AddEndpointsApiExplorer();
//    builder.Services.AddSwaggerGen();

//    // ── Authentication ──────────────────────────────────────────────
//    builder.Services.AddAuthentication(options =>
//    {
//        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//        options.DefaultChallengeScheme = OpenIddictClientAspNetCoreDefaults.AuthenticationScheme;
//    })
//    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
//    {
//        options.LoginPath = "/api/auth/login";
//        options.LogoutPath = "/api/auth/logout";
//        options.ExpireTimeSpan = TimeSpan.FromHours(8);
//        options.SlidingExpiration = true;
//        options.Cookie.HttpOnly = true;
//        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//        options.Cookie.SameSite = SameSiteMode.Strict;
//    });

//    // ── OpenTelemetry ───────────────────────────────────────────────
//    builder.Services.AddOpenTelemetry()
//        .ConfigureResource(resource => resource.AddService("BankingGateway"))
//        .WithTracing(tracing => tracing
//            .AddAspNetCoreInstrumentation()
//            .AddHttpClientInstrumentation())
//        .WithMetrics(metrics => metrics
//            .AddAspNetCoreInstrumentation()
//            .AddHttpClientInstrumentation()
//            .AddPrometheusExporter());

//    // ── OpenIddict Client + Validation (FINAL FIXED VERSION) ────────
//    builder.Services.AddOpenIddict()
//        .AddClient(options =>
//        {
//            // 1. Enable the flow we need
//            options.AllowAuthorizationCodeFlow();

//            // 2. Development keys (required for interactive login)
//            options.AddDevelopmentEncryptionCertificate();
//            options.AddDevelopmentSigningCertificate();

//            // 3. Discovery client
//            options.UseSystemNetHttp();

//            options.UseAspNetCore()
//                   .EnableRedirectionEndpointPassthrough()
//                   .DisableTransportSecurityRequirement(); // Only for dev

//            options.AddRegistration(new OpenIddictClientRegistration
//            {
//                Issuer = new Uri(builder.Configuration["OpenIddict:Issuer"] ?? "https://localhost:7001"),
//                ClientId = "bankinggateway-api",
//                ClientSecret = builder.Configuration["OpenIddict:Clients:BankingGatewayApi:Secret"]
//                    ?? "super-strong-secret-change-in-production",

//                RedirectUri = new Uri("https://localhost:5001/api/auth/callback"),

//                Scopes =
//                {
//                    OpenIddictConstants.Scopes.OpenId,
//                    OpenIddictConstants.Scopes.Profile,
//                    OpenIddictConstants.Scopes.Email,
//                    OpenIddictConstants.Scopes.OfflineAccess,
//                    "banking_api"
//                }
//            });
//        })
//        .AddValidation(options =>
//        {
//            options.SetIssuer(jwtSettings.Authority);
//            options.AddAudiences(jwtSettings.Audience);

//            options.UseIntrospection()
//                   .SetClientId(jwtSettings.Audience)
//                   .SetClientSecret(builder.Configuration["OpenIddict:GatewayClientSecret"]
//                       ?? throw new InvalidOperationException("OpenIddict:GatewayClientSecret is required."));

//            options.UseSystemNetHttp();
//            options.UseAspNetCore();
//        });

//    // ── Authorization ───────────────────────────────────────────────
//    builder.Services.AddAuthorization(options =>
//    {
//        options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
//        options.AddPolicy("ReadAccess", p => p.RequireAuthenticatedUser());
//        options.AddPolicy("WriteAccess", p => p.RequireRole("Admin", "Writer"));
//        options.AddPolicy("BankingApi", p => p.RequireClaim("scope", "banking_api"));
//    });

//    builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

//    // ── Rate Limiting ───────────────────────────────────────────────
//    builder.Services.AddRateLimiter(options =>
//    {
//        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
//        options.AddFixedWindowLimiter("fixed", opt =>
//        {
//            opt.PermitLimit = rateLimitSettings.PermitLimit;
//            opt.Window = TimeSpan.FromSeconds(rateLimitSettings.WindowSeconds);
//            opt.QueueLimit = rateLimitSettings.QueueLimit;
//            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//        });
//    });

//    // ── YARP ────────────────────────────────────────────────────────
//    builder.Services.AddReverseProxy()
//        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

//    // ── Health Checks ───────────────────────────────────────────────
//    var healthChecksBuilder = builder.Services.AddHealthChecks();

//    var redisConfig = builder.Configuration.GetSection(RedisSettings.SectionName).Get<RedisSettings>();
//    if (redisConfig is { Enabled: true } && !string.IsNullOrWhiteSpace(redisConfig.ConnectionString))
//    {
//        healthChecksBuilder.AddRedis(redisConfig.ConnectionString, name: "redis");
//    }

//    var issuer = builder.Configuration["OpenIddict:Issuer"] ?? "https://localhost:7001";
//    healthChecksBuilder.AddUrlGroup(
//        new Uri(issuer.TrimEnd('/') + "/.well-known/openid-configuration"),
//        name: "identityserver",
//        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded);

//    // ═════════════════════════════════════════════════════════════════
//    var app = builder.Build();

//    // ── Middleware ──────────────────────────────────────────────────
//    app.UseMiddleware<ExceptionHandlingMiddleware>();
//    app.UseMiddleware<CorrelationIdMiddleware>();
//    app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/swagger"),
//        appBuilder => appBuilder.UseMiddleware<SecurityHeadersMiddleware>());

//    app.UseMiddleware<IpFilteringMiddleware>();
//    app.UseMiddleware<RequestSizeLimitMiddleware>();

//    if (!app.Environment.IsDevelopment())
//        app.UseHsts();

//    app.UseHttpsRedirection();

//    if (app.Environment.IsDevelopment())
//    {
//        app.UseSwagger();
//        app.UseSwaggerUI();
//    }

//    app.UseRateLimiter();
//    app.UseAuthentication();
//    app.UseAuthorization();

//    app.MapControllers();
//    app.MapReverseProxy();

//    app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
//    app.MapHealthChecks("/health/ready");

//    app.UseOpenTelemetryPrometheusScrapingEndpoint("/metrics");

//    app.UseSerilogRequestLogging();

//    app.Run();
//}
//catch (Exception ex)
//{
//    Log.Fatal(ex, "Application terminated unexpectedly");
//}
//finally
//{
//    Log.CloseAndFlush();
//}

//public partial class Program { }