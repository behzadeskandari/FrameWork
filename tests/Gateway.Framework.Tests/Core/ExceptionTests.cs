using Gateway.Framework.Core.Exceptions;
using Gateway.Framework.Core.Models;
using Xunit;

namespace Gateway.Framework.Tests.Core;

public class ExceptionTests
{
    [Fact]
    public void DomainException_ShouldHave_CorrectProperties()
    {
        var ex = new DomainException("Test error", BankingErrorCodes.GeneralError, 400);
        
        Assert.Equal("Test error", ex.Message);
        Assert.Equal(BankingErrorCodes.GeneralError, ex.ErrorCode);
        Assert.Equal(400, ex.HttpStatusCode);
    }

    [Fact]
    public void NotFoundException_ShouldBe_404()
    {
        var ex = new NotFoundException("User", 123);
        
        Assert.Equal(404, ex.HttpStatusCode);
        Assert.Equal(BankingErrorCodes.ResourceNotFound, ex.ErrorCode);
        Assert.Contains("User", ex.Message);
        Assert.Contains("123", ex.Message);
    }

    [Fact]
    public void UnauthorizedException_ShouldBe_401()
    {
        var ex = new UnauthorizedException();
        
        Assert.Equal(401, ex.HttpStatusCode);
        Assert.Equal(BankingErrorCodes.AuthenticationFailed, ex.ErrorCode);
    }

    [Fact]
    public void ForbiddenException_ShouldBe_403()
    {
        var ex = new ForbiddenException();
        
        Assert.Equal(403, ex.HttpStatusCode);
        Assert.Equal(BankingErrorCodes.AuthorizationFailed, ex.ErrorCode);
    }

    [Fact]
    public void ServiceUnavailableException_ShouldBe_503()
    {
        var ex = new ServiceUnavailableException();
        
        Assert.Equal(503, ex.HttpStatusCode);
        Assert.Equal(BankingErrorCodes.ServiceUnavailable, ex.ErrorCode);
    }
}
