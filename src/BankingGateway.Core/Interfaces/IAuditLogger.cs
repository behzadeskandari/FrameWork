namespace BankingGateway.Core.Interfaces;

public interface IAuditLogger
{
    void LogAuditEvent(string userId, string action, string resource, string? detail = null);
    void LogSecurityEvent(string eventType, string message, string? sourceIp = null);
}
