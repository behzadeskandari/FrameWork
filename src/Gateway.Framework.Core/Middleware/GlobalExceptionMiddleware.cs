using System.Net;
using System.Text.Json;
using Gateway.Framework.Core.Exceptions;
using Gateway.Framework.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Gateway.Framework.Core.Middleware;

/// <summary>
/// Global exception handling middleware that maps exceptions to standardized API responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, errorCode, message) = exception switch
        {
            DomainException domainEx => (domainEx.HttpStatusCode, domainEx.ErrorCode, domainEx.Message),
            OperationCanceledException => ((int)HttpStatusCode.RequestTimeout, BankingErrorCodes.GeneralError, "The request was cancelled."),
            _ => ((int)HttpStatusCode.InternalServerError, BankingErrorCodes.GeneralError, "An unexpected error occurred.")
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);
        }
        else
        {
            _logger.LogWarning(exception, "Handled domain exception occurred. TraceId: {TraceId}, ErrorCode: {ErrorCode}", traceId, errorCode);
        }

        var response = ApiResponse.Fail(message, statusCode, new List<ApiError>
        {
            new(errorCode, message)
        });
        response.TraceId = traceId;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
