using System.Net;
using Gateway.Framework.Shared.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gateway.Framework.Security.Middleware;

/// <summary>
/// Middleware that restricts access based on IP whitelist.
/// </summary>
public class IpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpWhitelistMiddleware> _logger;
    private readonly SecuritySettings _settings;

    public IpWhitelistMiddleware(
        RequestDelegate next,
        ILogger<IpWhitelistMiddleware> logger,
        IOptions<SecuritySettings> settings)
    {
        _next = next;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_settings.AllowedIps.Count == 0)
        {
            await _next(context);
            return;
        }

        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp == null)
        {
            _logger.LogWarning("Request rejected: Remote IP address is null.");
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return;
        }

        var ipString = remoteIp.MapToIPv4().ToString();
        if (!_settings.AllowedIps.Contains(ipString))
        {
            _logger.LogWarning("Request rejected from IP: {IpAddress}", ipString);
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return;
        }

        await _next(context);
    }
}
