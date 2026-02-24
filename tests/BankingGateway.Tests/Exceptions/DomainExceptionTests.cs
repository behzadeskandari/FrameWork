using BankingGateway.Core.Exceptions;
using FluentAssertions;

namespace BankingGateway.Tests.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void DomainException_Should_Have_Default_StatusCode_400()
    {
        var ex = new DomainException("Bad request");

        ex.StatusCode.Should().Be(400);
        ex.Message.Should().Be("Bad request");
    }

    [Fact]
    public void NotFoundException_Should_Have_StatusCode_404()
    {
        var ex = new NotFoundException("Not found");

        ex.StatusCode.Should().Be(404);
    }

    [Fact]
    public void UnauthorizedException_Should_Have_StatusCode_401()
    {
        var ex = new UnauthorizedException("Unauthorized");

        ex.StatusCode.Should().Be(401);
    }

    [Fact]
    public void ForbiddenException_Should_Have_StatusCode_403()
    {
        var ex = new ForbiddenException("Forbidden");

        ex.StatusCode.Should().Be(403);
    }

    [Fact]
    public void ConflictException_Should_Have_StatusCode_409()
    {
        var ex = new ConflictException("Conflict");

        ex.StatusCode.Should().Be(409);
    }
}
