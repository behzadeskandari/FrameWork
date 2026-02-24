namespace BankingGateway.Core.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = true;
}
