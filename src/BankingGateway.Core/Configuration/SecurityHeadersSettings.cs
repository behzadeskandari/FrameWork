namespace BankingGateway.Core.Configuration;

public sealed class SecurityHeadersSettings
{
    public const string SectionName = "SecurityHeaders";
    public bool EnableHsts { get; set; } = true;
    public int HstsMaxAgeSeconds { get; set; } = 31536000;
    public long MaxRequestBodySize { get; set; } = 10_485_760; // 10 MB
    public List<string> AllowedIpRanges { get; set; } = new();
    public bool EnableIpFiltering { get; set; } = false;
}
