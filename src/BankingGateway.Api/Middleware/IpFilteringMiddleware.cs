using System.Net;
using BankingGateway.Core.Configuration;
using Microsoft.Extensions.Options;

namespace BankingGateway.Api.Middleware;

public sealed class IpFilteringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpFilteringMiddleware> _logger;
    private readonly SecurityHeadersSettings _settings;

    public IpFilteringMiddleware(RequestDelegate next, ILogger<IpFilteringMiddleware> logger,
        IOptions<SecurityHeadersSettings> settings)
    {
        _next = next;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_settings.EnableIpFiltering || _settings.AllowedIpRanges.Count == 0)
        {
            await _next(context);
            return;
        }

        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return;
        }

        var ipString = remoteIp.MapToIPv4().ToString();
        if (!_settings.AllowedIpRanges.Contains(ipString))
        {
            _logger.LogWarning("Blocked request from IP {RemoteIp}", ipString);
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return;
        }

        await _next(context);
    }
}
