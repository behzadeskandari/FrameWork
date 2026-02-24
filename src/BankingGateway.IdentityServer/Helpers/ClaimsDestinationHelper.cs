#nullable enable

using System.Security.Claims;
using OpenIddict.Abstractions;

namespace BankingGateway.IdentityServer.Helpers;

/// <summary>
/// Determines correct OpenIddict token destinations for each claim type.
/// </summary>
public static class ClaimsDestinationHelper
{
    /// <summary>
    /// Returns the token destinations for a given claim.
    /// </summary>
    public static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        return claim.Type switch
        {
            OpenIddictConstants.Claims.Name
                or OpenIddictConstants.Claims.Email
                or OpenIddictConstants.Claims.EmailVerified
                => [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],

            OpenIddictConstants.Claims.Role
                => [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],

            OpenIddictConstants.Claims.Subject
                => [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],

            _ => [OpenIddictConstants.Destinations.AccessToken]
        };
    }
}
