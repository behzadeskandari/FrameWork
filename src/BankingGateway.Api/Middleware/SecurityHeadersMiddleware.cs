namespace BankingGateway.Api.Middleware;

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var nonce = Guid.NewGuid().ToString("N");
        context.Items["CSPNonce"] = nonce;

        context.Response.OnStarting(() =>
        {
           
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["X-XSS-Protection"] = "1; mode=block";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            //headers["Content-Security-Policy"] = "default-src 'self'";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            headers.Remove("X-Powered-By");
            headers.Remove("Server");
            // Include the nonce in the policy
            headers["Content-Security-Policy"] = $"default-src 'self'; script-src 'self' 'nonce-{nonce}'; style-src 'self' 'unsafe-inline';";
            // ... rest of your headers
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
