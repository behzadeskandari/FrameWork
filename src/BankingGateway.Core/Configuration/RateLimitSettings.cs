namespace BankingGateway.Core.Configuration;

public sealed class RateLimitSettings
{
    public const string SectionName = "RateLimit";
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 0;
}
