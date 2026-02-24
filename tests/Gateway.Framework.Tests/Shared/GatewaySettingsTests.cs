using Gateway.Framework.Shared.Configuration;
using Xunit;

namespace Gateway.Framework.Tests.Shared;

public class GatewaySettingsTests
{
    [Fact]
    public void DefaultSettings_ShouldHaveExpectedValues()
    {
        var settings = new GatewaySettings();

        Assert.Equal("Banking Gateway", settings.Name);
        Assert.Equal("1.0.0", settings.Version);
        Assert.Equal("Development", settings.Environment);
        Assert.NotNull(settings.Security);
        Assert.NotNull(settings.Cache);
        Assert.NotNull(settings.Resilience);
    }

    [Fact]
    public void SecuritySettings_Defaults_ShouldBeSecure()
    {
        var settings = new SecuritySettings();

        Assert.True(settings.EnableHttpsRedirection);
        Assert.True(settings.EnableHsts);
        Assert.True(settings.RateLimit.Enabled);
    }

    [Fact]
    public void JwtSettings_Defaults_ShouldBeSecure()
    {
        var settings = new JwtSettings();

        Assert.True(settings.ValidateIssuer);
        Assert.True(settings.ValidateAudience);
        Assert.True(settings.ValidateLifetime);
        Assert.True(settings.RequireHttpsMetadata);
    }

    [Fact]
    public void ResilienceSettings_ShouldHaveReasonableDefaults()
    {
        var settings = new ResilienceSettings();

        Assert.Equal(3, settings.RetryCount);
        Assert.Equal(30, settings.TimeoutSeconds);
        Assert.True(settings.CircuitBreakerThreshold > 0);
    }
}
