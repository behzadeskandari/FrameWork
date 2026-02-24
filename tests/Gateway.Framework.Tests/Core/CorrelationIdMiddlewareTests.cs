using Gateway.Framework.Core.Middleware;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Gateway.Framework.Tests.Core;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task Should_GenerateCorrelationId_WhenNotProvided()
    {
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.True(context.Items.ContainsKey("CorrelationId"));
        Assert.NotNull(context.Items["CorrelationId"]);
    }

    [Fact]
    public async Task Should_UseExistingCorrelationId_WhenProvided()
    {
        var context = new DefaultHttpContext();
        var expectedId = "test-correlation-id";
        context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeader] = expectedId;
        
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(expectedId, context.Items["CorrelationId"]?.ToString());
    }
}
