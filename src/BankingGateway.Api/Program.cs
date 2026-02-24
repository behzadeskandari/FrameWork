using BankingGateway.Api.Auth;
using BankingGateway.Api.Middleware;
using BankingGateway.Api.Validation;
using BankingGateway.Core.Configuration;
using BankingGateway.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

// ── Serilog bootstrap ──────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/gateway-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "BankingGateway")
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File("logs/gateway-.log", rollingInterval: RollingInterval.Day));

    // ── Configuration binding ───────────────────────────────────────
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
    var securitySettings = builder.Configuration.GetSection(SecurityHeadersSettings.SectionName).Get<SecurityHeadersSettings>() ?? new SecurityHeadersSettings();
    var rateLimitSettings = builder.Configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>() ?? new RateLimitSettings();

    // ── Infrastructure services ─────────────────────────────────────
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Controllers + validation filter ─────────────────────────────
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    });
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // ── Swagger / OpenAPI ───────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ── Authentication (JWT Bearer) ─────────────────────────────────
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = jwtSettings.Authority;
            options.Audience = jwtSettings.Audience;
            options.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

    // Claims transformation
    builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

    // ── Authorization (role & policy based) ─────────────────────────
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("ReadAccess", policy => policy.RequireAuthenticatedUser());
        options.AddPolicy("WriteAccess", policy => policy.RequireRole("Admin", "Writer"));
    });

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

    // ── Request size limit (Kestrel) ────────────────────────────────
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = securitySettings.MaxRequestBodySize;
    });

    // ══════════════════════════════════════════════════════════════════
    var app = builder.Build();

    // ── Exception handling (outermost) ──────────────────────────────
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // ── Correlation ID ──────────────────────────────────────────────
    app.UseMiddleware<CorrelationIdMiddleware>();

    // ── Security headers ────────────────────────────────────────────
    app.UseMiddleware<SecurityHeadersMiddleware>();

    // ── IP Filtering ────────────────────────────────────────────────
    app.UseMiddleware<IpFilteringMiddleware>();

    // ── Request size limit ──────────────────────────────────────────
    app.UseMiddleware<RequestSizeLimitMiddleware>();

    // ── HTTPS & HSTS ────────────────────────────────────────────────
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }
    app.UseHttpsRedirection();

    // ── Swagger (dev only) ──────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // ── Rate limiting ───────────────────────────────────────────────
    app.UseRateLimiter();

    // ── Authentication & Authorization ──────────────────────────────
    app.UseAuthentication();
    app.UseAuthorization();

    // ── Controllers ─────────────────────────────────────────────────
    app.MapControllers();

    // ── YARP Reverse Proxy endpoints ────────────────────────────────
    app.MapReverseProxy();

    // ── Health checks ───────────────────────────────────────────────
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false // Liveness: always healthy if process is running
    });
    app.MapHealthChecks("/health/ready");

    // ── Prometheus metrics endpoint ─────────────────────────────────
    app.MapPrometheusScrapingEndpoint("/metrics");

    // ── Serilog request logging ─────────────────────────────────────
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

// Required for WebApplicationFactory in integration tests
public partial class Program { }
