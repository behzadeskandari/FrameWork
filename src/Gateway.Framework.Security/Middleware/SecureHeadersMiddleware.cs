using Microsoft.AspNetCore.Http;

namespace Gateway.Framework.Security.Middleware;

/// <summary>
/// Middleware that adds security headers to all responses (banking-grade).
/// </summary>
public class SecureHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecureHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["X-XSS-Protection"] = "1; mode=block";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Content-Security-Policy"] = "default-src 'self'";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            headers["Pragma"] = "no-cache";
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
