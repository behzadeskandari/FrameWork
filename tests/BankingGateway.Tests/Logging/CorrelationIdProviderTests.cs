using BankingGateway.Infrastructure.Logging;
using FluentAssertions;

namespace BankingGateway.Tests.Logging;

public class CorrelationIdProviderTests
{
    [Fact]
    public void GetCorrelationId_Should_Return_Value_After_Set()
    {
        var provider = new CorrelationIdProvider();
        provider.SetCorrelationId("test-123");

        var result = provider.GetCorrelationId();

        result.Should().Be("test-123");
    }

    [Fact]
    public void GetCorrelationId_Should_Generate_Id_When_Not_Set()
    {
        var provider = new CorrelationIdProvider();

        var result = provider.GetCorrelationId();

        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(32); // GUID without hyphens
    }
}
