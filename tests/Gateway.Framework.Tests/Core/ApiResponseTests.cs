using Gateway.Framework.Core.Models;
using Xunit;

namespace Gateway.Framework.Tests.Core;

public class ApiResponseTests
{
    [Fact]
    public void Ok_ShouldReturn_SuccessResponse()
    {
        var response = ApiResponse.Ok(new { Id = 1 }, "Success");
        
        Assert.True(response.Success);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public void Created_ShouldReturn_201Response()
    {
        var response = ApiResponse.Created(new { Id = 1 });
        
        Assert.True(response.Success);
        Assert.Equal(201, response.StatusCode);
    }

    [Fact]
    public void Fail_ShouldReturn_ErrorResponse()
    {
        var response = ApiResponse.Fail("Something went wrong", 400);
        
        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Something went wrong", response.Message);
    }

    [Fact]
    public void ValidationFail_ShouldReturn_422Response()
    {
        var errors = new List<ApiError>
        {
            new(BankingErrorCodes.ValidationError, "Field is required", "Name")
        };
        
        var response = ApiResponse.ValidationFail(errors);
        
        Assert.False(response.Success);
        Assert.Equal(422, response.StatusCode);
        Assert.Single(response.Errors!);
    }

    [Fact]
    public void GenericOk_ShouldReturn_TypedResponse()
    {
        var data = new { Name = "Test" };
        var response = ApiResponse<object>.Ok(data, "OK");
        
        Assert.True(response.Success);
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public void Timestamp_ShouldBeSetToUtcNow()
    {
        var before = DateTime.UtcNow;
        var response = ApiResponse.Ok();
        var after = DateTime.UtcNow;
        
        Assert.InRange(response.Timestamp, before, after);
    }
}
