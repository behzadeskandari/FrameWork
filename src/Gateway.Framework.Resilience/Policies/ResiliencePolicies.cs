using Gateway.Framework.Shared.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Gateway.Framework.Resilience.Policies;

/// <summary>
/// Configures resilience policies (retry, circuit breaker, timeout) using Polly.
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Creates a retry policy with exponential backoff.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ResilienceSettings settings, ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                settings.RetryCount,
                retryAttempt => TimeSpan.FromMilliseconds(settings.RetryBaseDelayMs * Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    logger.LogWarning(
                        "Retry {RetryAttempt} after {Delay}ms. Status: {StatusCode}",
                        retryAttempt,
                        timespan.TotalMilliseconds,
                        outcome.Result?.StatusCode);
                });
    }

    /// <summary>
    /// Creates a circuit breaker policy.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ResilienceSettings settings, ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                settings.CircuitBreakerThreshold,
                TimeSpan.FromSeconds(settings.CircuitBreakerDurationSeconds),
                onBreak: (outcome, timespan) =>
                {
                    logger.LogError("Circuit breaker opened for {Duration}s. Reason: {Reason}",
                        timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker reset.");
                });
    }

    /// <summary>
    /// Creates a timeout policy.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(ResilienceSettings settings)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(settings.TimeoutSeconds));
    }
}
