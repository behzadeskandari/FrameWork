using BankingGateway.Core.Configuration;
using FluentAssertions;

namespace BankingGateway.Tests.Configuration;

public class ConfigurationTests
{
    [Fact]
    public void JwtSettings_Should_Have_Default_Values()
    {
        var settings = new JwtSettings();

        settings.Authority.Should().BeEmpty();
        settings.Audience.Should().BeEmpty();
        settings.RequireHttpsMetadata.Should().BeTrue();
    }

    [Fact]
    public void RateLimitSettings_Should_Have_Defaults()
    {
        var settings = new RateLimitSettings();

        settings.PermitLimit.Should().Be(100);
        settings.WindowSeconds.Should().Be(60);
        settings.QueueLimit.Should().Be(0);
    }

    [Fact]
    public void ResilienceSettings_Should_Have_Defaults()
    {
        var settings = new ResilienceSettings();

        settings.RetryCount.Should().Be(3);
        settings.CircuitBreakerThreshold.Should().Be(5);
        settings.CircuitBreakerDurationSeconds.Should().Be(30);
        settings.TimeoutSeconds.Should().Be(30);
    }

    [Fact]
    public void SecurityHeadersSettings_Should_Have_Defaults()
    {
        var settings = new SecurityHeadersSettings();

        settings.EnableHsts.Should().BeTrue();
        settings.MaxRequestBodySize.Should().Be(10_485_760);
        settings.EnableIpFiltering.Should().BeFalse();
    }

    [Fact]
    public void RedisSettings_Should_Have_Defaults()
    {
        var settings = new RedisSettings();

        settings.Enabled.Should().BeFalse();
        settings.ConnectionString.Should().BeEmpty();
        settings.InstanceName.Should().Be("BankingGateway_");
    }
}
