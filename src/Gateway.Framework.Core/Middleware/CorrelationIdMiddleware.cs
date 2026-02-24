using Microsoft.AspNetCore.Http;

namespace Gateway.Framework.Core.Middleware;

/// <summary>
/// Middleware that ensures every request has a correlation ID for distributed tracing.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    public const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) ||
            string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("D");
        }

        context.Items["CorrelationId"] = correlationId.ToString();
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId.ToString();
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
