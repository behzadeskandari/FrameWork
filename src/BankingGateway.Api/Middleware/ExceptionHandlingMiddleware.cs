using System.Net;
using System.Text.Json;
using BankingGateway.Core.Exceptions;
using BankingGateway.Core.Interfaces;

namespace BankingGateway.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdProvider correlationIdProvider)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = correlationIdProvider.GetCorrelationId();
            _logger.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);

            context.Response.ContentType = "application/json";

            var (statusCode, message) = ex switch
            {
                DomainException domainEx => (domainEx.StatusCode, domainEx.Message),
                OperationCanceledException => ((int)HttpStatusCode.RequestTimeout, "Request was cancelled."),
                _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            context.Response.StatusCode = statusCode;

            var response = JsonSerializer.Serialize(new
            {
                error = message,
                correlationId,
                statusCode
            });

            await context.Response.WriteAsync(response);
        }
    }
}
