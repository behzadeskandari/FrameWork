namespace Gateway.Framework.Core.Interfaces;

/// <summary>
/// Interface for audit logging operations.
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(string action, string userId, string? detail = null, string? ipAddress = null);
    Task LogSecurityEventAsync(string eventType, string? userId, string? detail = null, string? ipAddress = null);
}
