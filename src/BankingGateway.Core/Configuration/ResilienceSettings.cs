namespace BankingGateway.Core.Configuration;

public sealed class ResilienceSettings
{
    public const string SectionName = "Resilience";
    public int RetryCount { get; set; } = 3;
    public int RetryBaseDelayMs { get; set; } = 200;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
    public int TimeoutSeconds { get; set; } = 30;
}
