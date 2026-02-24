namespace Gateway.Framework.Shared.Configuration;

/// <summary>
/// Cache configuration.
/// </summary>
public class CacheSettings
{
    public bool UseDistributedCache { get; set; }
    public string RedisConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = "BankingGateway_";
    public int DefaultExpirationMinutes { get; set; } = 30;
}
