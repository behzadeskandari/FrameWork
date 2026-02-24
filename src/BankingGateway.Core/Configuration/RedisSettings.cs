namespace BankingGateway.Core.Configuration;

public sealed class RedisSettings
{
    public const string SectionName = "Redis";
    public bool Enabled { get; set; } = false;
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = "BankingGateway_";
}
