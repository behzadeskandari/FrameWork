using Gateway.Framework.Logging.Middleware;
using Xunit;

namespace Gateway.Framework.Tests.Logging;

public class SensitiveDataMaskingTests
{
    [Fact]
    public void ShouldMask_PasswordField()
    {
        var input = """{"username":"john","password":"secret123"}""";
        var result = RequestResponseLoggingMiddleware.MaskSensitiveData(input);
        
        Assert.DoesNotContain("secret123", result);
        Assert.Contains("***MASKED***", result);
    }

    [Fact]
    public void ShouldMask_CreditCardField()
    {
        var input = """{"creditcard":"4111111111111111"}""";
        var result = RequestResponseLoggingMiddleware.MaskSensitiveData(input);
        
        Assert.DoesNotContain("4111111111111111", result);
        Assert.Contains("***MASKED***", result);
    }

    [Fact]
    public void ShouldNotMask_NonSensitiveFields()
    {
        var input = """{"username":"john","email":"john@test.com"}""";
        var result = RequestResponseLoggingMiddleware.MaskSensitiveData(input);
        
        Assert.Contains("john", result);
        Assert.Contains("john@test.com", result);
    }

    [Fact]
    public void ShouldReturnEmpty_ForEmptyInput()
    {
        var result = RequestResponseLoggingMiddleware.MaskSensitiveData("");
        Assert.Equal("", result);
    }

    [Fact]
    public void ShouldReturnNull_ForNullInput()
    {
        var result = RequestResponseLoggingMiddleware.MaskSensitiveData(null!);
        Assert.Null(result);
    }
}
