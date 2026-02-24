using System.Net;
using Gateway.Framework.Shared.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Gateway.Framework.Security.Middleware;

/// <summary>
/// Middleware that enforces request body size limits.
/// </summary>
public class RequestSizeLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly long _maxRequestBodySize;

    public RequestSizeLimitMiddleware(RequestDelegate next, IOptions<SecuritySettings> settings)
    {
        _next = next;
        _maxRequestBodySize = settings.Value.MaxRequestBodySize;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.ContentLength.HasValue && context.Request.ContentLength.Value > _maxRequestBodySize)
        {
            context.Response.StatusCode = (int)HttpStatusCode.RequestEntityTooLarge;
            await context.Response.WriteAsync("Request body too large.");
            return;
        }

        await _next(context);
    }
}
