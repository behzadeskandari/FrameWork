using BankingGateway.Core.Interfaces;
using BankingGateway.Infrastructure.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BankingGateway.Tests.Logging;

public class AuditLoggerTests
{
    private readonly Mock<ILogger<AuditLogger>> _loggerMock;
    private readonly Mock<ICorrelationIdProvider> _correlationMock;
    private readonly AuditLogger _sut;

    public AuditLoggerTests()
    {
        _loggerMock = new Mock<ILogger<AuditLogger>>();
        _correlationMock = new Mock<ICorrelationIdProvider>();
        _correlationMock.Setup(c => c.GetCorrelationId()).Returns("corr-123");
        _sut = new AuditLogger(_loggerMock.Object, _correlationMock.Object);
    }

    [Fact]
    public void LogAuditEvent_Should_Not_Throw()
    {
        var act = () => _sut.LogAuditEvent("user1", "Login", "/api/auth");

        act.Should().NotThrow();
    }

    [Fact]
    public void LogSecurityEvent_Should_Not_Throw()
    {
        var act = () => _sut.LogSecurityEvent("BruteForce", "Multiple failed attempts", "192.168.1.1");

        act.Should().NotThrow();
    }
}
