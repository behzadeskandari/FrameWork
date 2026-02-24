namespace Gateway.Framework.Shared.Configuration;

/// <summary>
/// Rate limiting configuration.
/// </summary>
public class RateLimitSettings
{
    public bool Enabled { get; set; } = true;
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 10;
}
