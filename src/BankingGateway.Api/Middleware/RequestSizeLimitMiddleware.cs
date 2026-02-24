using BankingGateway.Core.Configuration;
using Microsoft.Extensions.Options;

namespace BankingGateway.Api.Middleware;

public sealed class RequestSizeLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly long _maxRequestBodySize;

    public RequestSizeLimitMiddleware(RequestDelegate next, IOptions<SecurityHeadersSettings> settings)
    {
        _next = next;
        _maxRequestBodySize = settings.Value.MaxRequestBodySize;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.ContentLength > _maxRequestBodySize)
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            await context.Response.WriteAsync("Request body too large.");
            return;
        }

        var feature = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
        if (feature is not null && !feature.IsReadOnly)
        {
            feature.MaxRequestBodySize = _maxRequestBodySize;
        }

        await _next(context);
    }
}
