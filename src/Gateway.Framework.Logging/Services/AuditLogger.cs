using Gateway.Framework.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gateway.Framework.Logging.Services;

/// <summary>
/// Implementation of audit logging for security and compliance.
/// </summary>
public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(ILogger<AuditLogger> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task LogAsync(string action, string userId, string? detail = null, string? ipAddress = null)
    {
        _logger.LogInformation(
            "AUDIT: Action={Action}, UserId={UserId}, Detail={Detail}, IP={IpAddress}, Timestamp={Timestamp}",
            action, userId, detail ?? "N/A", ipAddress ?? "N/A", DateTime.UtcNow);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task LogSecurityEventAsync(string eventType, string? userId, string? detail = null, string? ipAddress = null)
    {
        _logger.LogWarning(
            "SECURITY: Event={EventType}, UserId={UserId}, Detail={Detail}, IP={IpAddress}, Timestamp={Timestamp}",
            eventType, userId ?? "anonymous", detail ?? "N/A", ipAddress ?? "N/A", DateTime.UtcNow);
        return Task.CompletedTask;
    }
}
