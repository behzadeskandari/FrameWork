using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Gateway.Framework.Logging.Middleware;

/// <summary>
/// Middleware for logging request/response with sensitive data masking.
/// </summary>
public partial class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    private static readonly string[] SensitiveFields = 
    { 
        "password", "secret", "token", "authorization", "credit_card",
        "creditcard", "card_number", "cardnumber", "cvv", "ssn",
        "social_security", "pin", "account_number", "accountnumber"
    };

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";

        _logger.LogInformation(
            "HTTP {Method} {Path} started. CorrelationId: {CorrelationId}, RemoteIP: {RemoteIp}",
            context.Request.Method,
            context.Request.Path,
            correlationId,
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "HTTP {Method} {Path} completed. StatusCode: {StatusCode}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);

        if (stopwatch.ElapsedMilliseconds > 5000)
        {
            _logger.LogWarning(
                "Slow request detected: {Method} {Path} took {Duration}ms. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }

    /// <summary>
    /// Masks sensitive fields in text content.
    /// </summary>
    public static string MaskSensitiveData(string content)
    {
        if (string.IsNullOrEmpty(content)) return content;

        foreach (var field in SensitiveFields)
        {
            var pattern = $@"(""{field}""\s*:\s*"")[^""]*("")";
            content = Regex.Replace(content, pattern, "${1}***MASKED***${2}", RegexOptions.IgnoreCase);
        }

        return content;
    }
}
