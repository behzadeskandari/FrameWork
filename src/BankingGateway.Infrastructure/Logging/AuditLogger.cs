using BankingGateway.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BankingGateway.Infrastructure.Logging;

public sealed class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    public AuditLogger(ILogger<AuditLogger> logger, ICorrelationIdProvider correlationIdProvider)
    {
        _logger = logger;
        _correlationIdProvider = correlationIdProvider;
    }

    public void LogAuditEvent(string userId, string action, string resource, string? detail = null)
    {
        _logger.LogInformation(
            "[AUDIT] CorrelationId={CorrelationId} User={UserId} Action={Action} Resource={Resource} Detail={Detail}",
            _correlationIdProvider.GetCorrelationId(), userId, action, resource, detail ?? "N/A");
    }

    public void LogSecurityEvent(string eventType, string message, string? sourceIp = null)
    {
        _logger.LogWarning(
            "[SECURITY] CorrelationId={CorrelationId} EventType={EventType} Message={Message} SourceIp={SourceIp}",
            _correlationIdProvider.GetCorrelationId(), eventType, message, sourceIp ?? "unknown");
    }
}
