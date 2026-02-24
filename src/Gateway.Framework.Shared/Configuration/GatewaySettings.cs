namespace Gateway.Framework.Shared.Configuration;

/// <summary>
/// Root configuration for the banking gateway.
/// </summary>
public class GatewaySettings
{
    public const string SectionName = "Gateway";
    public string Name { get; set; } = "Banking Gateway";
    public string Version { get; set; } = "1.0.0";
    public string Environment { get; set; } = "Development";
    public SecuritySettings Security { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
    public CacheSettings Cache { get; set; } = new();
    public ResilienceSettings Resilience { get; set; } = new();
}
