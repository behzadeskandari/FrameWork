using BankingGateway.Api.Middleware;
using BankingGateway.Core.Exceptions;
using BankingGateway.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace BankingGateway.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
    private readonly Mock<ICorrelationIdProvider> _correlationMock;

    public ExceptionHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _correlationMock = new Mock<ICorrelationIdProvider>();
        _correlationMock.Setup(c => c.GetCorrelationId()).Returns("test-corr");
    }

    [Fact]
    public async Task Should_Return_400_For_DomainException()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new DomainException("Bad input"), _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _correlationMock.Object);

        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Should_Return_404_For_NotFoundException()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new NotFoundException("Not found"), _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _correlationMock.Object);

        context.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Should_Return_500_For_Unhandled_Exception()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("oops"), _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _correlationMock.Object);

        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Response_Should_Contain_CorrelationId()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new DomainException("test"), _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _correlationMock.Object);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("correlationId").GetString().Should().Be("test-corr");
    }
}
