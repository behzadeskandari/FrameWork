namespace Gateway.Framework.Shared.Configuration;

/// <summary>
/// Security configuration for the gateway.
/// </summary>
public class SecuritySettings
{
    public bool EnableHttpsRedirection { get; set; } = true;
    public bool EnableHsts { get; set; } = true;
    public JwtSettings Jwt { get; set; } = new();
    public RateLimitSettings RateLimit { get; set; } = new();
    public List<string> AllowedIps { get; set; } = new();
    public long MaxRequestBodySize { get; set; } = 10 * 1024 * 1024; // 10MB
    public List<string> AllowedOrigins { get; set; } = new();
}
