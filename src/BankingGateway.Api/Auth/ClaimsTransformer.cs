using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace BankingGateway.Api.Auth;

public sealed class ClaimsTransformer : IClaimsTransformation
{
    private readonly ILogger<ClaimsTransformer> _logger;

    public ClaimsTransformer(ILogger<ClaimsTransformer> logger)
    {
        _logger = logger;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            return Task.FromResult(principal);

        // Map standard OIDC claims to application claims
        var sub = identity.FindFirst("sub")?.Value ?? identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is not null && !identity.HasClaim("app_user_id", sub))
        {
            identity.AddClaim(new Claim("app_user_id", sub));
        }

        // Map roles from realm_access or role claims
        var roleClaims = identity.FindAll("role")
            .Concat(identity.FindAll(ClaimTypes.Role))
            .Select(c => c.Value)
            .Distinct();

        foreach (var role in roleClaims)
        {
            if (!identity.HasClaim(ClaimTypes.Role, role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

        _logger.LogDebug("Claims transformed for user {UserId}", sub);
        return Task.FromResult(principal);
    }
}
