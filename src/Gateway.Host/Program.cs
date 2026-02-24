using Gateway.Framework.Core.Extensions;
using Gateway.Framework.Core.Middleware;
using Gateway.Framework.Gateway.Extensions;
using Gateway.Framework.Infrastructure.Extensions;
using Gateway.Framework.Logging.Extensions;
using Gateway.Framework.Logging.Middleware;
using Gateway.Framework.Logging.Setup;
using Gateway.Framework.Monitoring.Extensions;
using Gateway.Framework.Security.Extensions;
using Gateway.Framework.Security.Middleware;
using Gateway.Framework.Shared.Configuration;
using Gateway.Framework.Shared.Extensions;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.AddGatewayConfiguration(builder.Configuration);
var gatewaySettings = builder.Configuration
    .GetSection(GatewaySettings.SectionName)
    .Get<GatewaySettings>() ?? new GatewaySettings();

// Serilog
builder.AddGatewayLogging();

// Core Services
builder.Services.AddGatewayCore();

// Security
builder.Services.AddGatewaySecurity(gatewaySettings.Security);

// Logging Services
builder.Services.AddGatewayLoggingServices();

// Monitoring
builder.Services.AddGatewayMonitoring(gatewaySettings.Name);

// Infrastructure (Caching, Feature Flags)
builder.Services.AddGatewayInfrastructure(gatewaySettings.Cache);

// YARP Reverse Proxy
builder.Services.AddGatewayProxy(builder.Configuration);

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Banking Gateway API",
        Version = "v1",
        Description = "Banking Gateway Framework API - Enterprise-grade gateway for banking services."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments
    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
    foreach (var xmlFile in xmlFiles)
    {
        options.IncludeXmlComments(xmlFile);
    }
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (gatewaySettings.Security.AllowedOrigins.Count > 0)
        {
            policy.WithOrigins(gatewaySettings.Security.AllowedOrigins.ToArray())
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
        else if (builder.Environment.IsDevelopment())
        {
            // Only allow any origin in development; production must configure allowed origins
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
        else
        {
            // Restrictive default: no origins allowed in non-development environments
            policy.WithOrigins("https://localhost")
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Middleware Pipeline (order matters)
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecureHeadersMiddleware>();
app.UseMiddleware<RequestSizeLimitMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking Gateway API v1");
    });
}

app.UseCors();

app.UseGatewaySecurity(gatewaySettings.Security);

if (gatewaySettings.Security.RateLimit.Enabled)
{
    app.UseRateLimiter();
}

app.MapControllers();
app.MapGatewayMonitoring();
app.MapGatewayProxy();

app.Run();

// Make the implicit Program class public so test projects can access it
namespace Gateway.Host
{
    /// <summary>
    /// Entry point for integration testing support.
    /// </summary>
    public partial class Program { }
}
