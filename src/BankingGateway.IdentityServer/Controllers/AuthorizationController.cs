#nullable enable

using BankingGateway.IdentityServer.Domain;
using BankingGateway.IdentityServer.Helpers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace BankingGateway.IdentityServer.Controllers;

/// <summary>
/// Handles OpenID Connect endpoints: authorize, token, userinfo, logout, introspect.
/// </summary>
[ApiController]
public sealed class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // ── /connect/authorize ─────────────────────────────────────────────

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict request could not be retrieved.");

        // If not authenticated, redirect to login
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!result.Succeeded)
        {
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        var user = await _userManager.GetUserAsync(result.Principal)
            ?? throw new InvalidOperationException("The user details cannot be retrieved.");

        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!)
            ?? throw new InvalidOperationException("The application was not found.");

        var identity = new ClaimsIdentity(
            authenticationType: "Bearer",
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user))
                .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
                .SetClaim(OpenIddictConstants.Claims.Name, user.UserName)
                .SetClaims(OpenIddictConstants.Claims.Role,
                    [.. (await _userManager.GetRolesAsync(user))]);

        identity.SetScopes(request.GetScopes());

        var resources = new List<string>();
        await foreach (var resource in _scopeManager.ListResourcesAsync(identity.GetScopes()))
            resources.Add(resource);
        identity.SetResources(resources);

        identity.SetDestinations(claim =>
            ClaimsDestinationHelper.GetDestinations(claim, new ClaimsPrincipal(identity)));

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    // ── /connect/token ─────────────────────────────────────────────────

    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict request could not be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var user = await _userManager.FindByIdAsync(result.Principal!.GetClaim(OpenIddictConstants.Claims.Subject)!);
            if (user is null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                    }));
            }

            if (!await _signInManager.CanSignInAsync(user))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                    }));
            }

            var identity = new ClaimsIdentity(
                result.Principal!.Claims,
                authenticationType: "Bearer",
                nameType: OpenIddictConstants.Claims.Name,
                roleType: OpenIddictConstants.Claims.Role);

            identity.SetClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user))
                    .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
                    .SetClaim(OpenIddictConstants.Claims.Name, user.UserName)
                    .SetClaims(OpenIddictConstants.Claims.Role,
                        [.. (await _userManager.GetRolesAsync(user))]);

            identity.SetDestinations(claim =>
                ClaimsDestinationHelper.GetDestinations(claim, new ClaimsPrincipal(identity)));

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsClientCredentialsGrantType())
        {
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId!)
                ?? throw new InvalidOperationException("The application was not found.");

            var identity = new ClaimsIdentity(
                authenticationType: "Bearer",
                nameType: OpenIddictConstants.Claims.Name,
                roleType: OpenIddictConstants.Claims.Role);

            identity.SetClaim(OpenIddictConstants.Claims.Subject,
                await _applicationManager.GetClientIdAsync(application));
            identity.SetClaim(OpenIddictConstants.Claims.Name,
                await _applicationManager.GetDisplayNameAsync(application));

            identity.SetScopes(request.GetScopes());

            var resources = new List<string>();
            await foreach (var resource in _scopeManager.ListResourcesAsync(identity.GetScopes()))
                resources.Add(resource);
            identity.SetResources(resources);

            identity.SetDestinations(claim =>
                ClaimsDestinationHelper.GetDestinations(claim, new ClaimsPrincipal(identity)));

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    // ── /connect/userinfo ──────────────────────────────────────────────

    //[Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    //[HttpGet("~/connect/userinfo")]
    //[HttpPost("~/connect/userinfo")]
    //[IgnoreAntiforgeryToken]
    //public async Task<IActionResult> Userinfo()
    //{
    //    var user = await _userManager.GetUserAsync(User);
    //    if (user is null)
    //    {
    //        return Challenge(
    //            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
    //            properties: new AuthenticationProperties(new Dictionary<string, string?>
    //            {
    //                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
    //                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified access token is bound to an account that no longer exists."
    //            }));
    //    }

    //    var claims = new Dictionary<string, object>(StringComparer.Ordinal)
    //    {
    //        [OpenIddictConstants.Claims.Subject] = await _userManager.GetUserIdAsync(user)
    //    };

    //    if (User.HasScope(OpenIddictConstants.Scopes.Email))
    //    {
    //        claims[OpenIddictConstants.Claims.Email] = await _userManager.GetEmailAsync(user) ?? string.Empty;
    //        claims[OpenIddictConstants.Claims.EmailVerified] = user.EmailConfirmed;
    //    }

    //    if (User.HasScope(OpenIddictConstants.Scopes.Profile))
    //    {
    //        claims[OpenIddictConstants.Claims.Name] = user.UserName ?? string.Empty;
    //        claims["given_name"] = user.FirstName;
    //        claims["family_name"] = user.LastName;
    //    }

    //    if (User.HasScope(OpenIddictConstants.Scopes.Roles))
    //    {
    //        claims[OpenIddictConstants.Claims.Role] = await _userManager.GetRolesAsync(user);
    //    }

    //    return Ok(claims);
    //}


    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo"), HttpPost("~/connect/userinfo")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Userinfo()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = await _userManager.GetUserIdAsync(user)
        };

        if (User.HasScope(Scopes.Email))
        {
            claims[Claims.Email] = await _userManager.GetEmailAsync(user) ?? string.Empty;
            claims[Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
        }

        if (User.HasScope(Scopes.Profile))
        {
            claims[Claims.Name] = user.UserName ?? string.Empty;
            // Include any custom profile data here
        }

        if (User.HasScope(Scopes.Roles))
        {
            claims[Claims.Role] = await _userManager.GetRolesAsync(user);
        }

        return Ok(claims);
    }
    // ── /connect/logout ────────────────────────────────────────────────

    [HttpGet("~/connect/logout")]
    public IActionResult Logout() => View();

    [ActionName(nameof(Logout))]
    [HttpPost("~/connect/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost()
    {
        await _signInManager.SignOutAsync();
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties { RedirectUri = "/" });
    }
}
