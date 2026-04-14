using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using System.Security.Claims;

namespace BankingGateway.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Callback endpoint called by the OpenIddict Authorization Server 
        /// after successful login (redirect URI).
        /// This handles the authorization code exchange automatically via the client middleware.
        /// </summary>
        [HttpGet("callback")]
        [HttpPost("callback")]   // Support both GET and POST (some IdPs use POST)
        [IgnoreAntiforgeryToken] // Required for POST callbacks from external IdP
        public async Task<IActionResult> Callback()
        {
            //// Retrieve the authentication result validated by OpenIddict client middleware
            //var result = await HttpContext.AuthenticateAsync(
            //    OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

            //if (!result.Succeeded)
            //{
            //    _logger.LogWarning("Authentication failed in callback: {Error}", result.Failure?.Message);
            //    return BadRequest(new { error = "Authentication failed", details = result.Failure?.Message });
            //}

            //// The ClaimsPrincipal now contains all claims from ID Token + UserInfo endpoint (if configured)
            //var principal = result.Principal;

            //// Optional: Log or inspect claims for debugging
            //var claims = principal.Claims.Select(c => new { c.Type, c.Value }).ToList();
            //_logger.LogInformation("User authenticated successfully. Claims count: {Count}", claims.Count);

            //// Sign in the user using cookie authentication (recommended for web APIs that need session)
            //// You can also store tokens if you need refresh token rotation later
            //await HttpContext.SignInAsync(
            //    CookieAuthenticationDefaults.AuthenticationScheme,
            //    principal,
            //    new AuthenticationProperties
            //    {
            //        IsPersistent = true,
            //        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8), // Adjust based on your policy
            //        RedirectUri = "/api/auth/success" // Optional
            //    });

            //// Redirect to your new client app (Task 3) or a success endpoint
            //// You can also return JSON if this is purely an API-to-API scenario
            //return Redirect("/api/auth/success");
            //// Alternative for API-only: return Ok(new { message = "Login successful", claims });
            // Retrieve the result specifically from the OpenIddict Client handler
            var result = await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

            if (result.Principal is null) return BadRequest("Invalid login.");

            // Task 3: Capture tokens to pass to other apps
            var props = new AuthenticationProperties { IsPersistent = true };

            // This allows you to call 'HttpContext.GetTokenAsync("access_token")' later
            var tokens = new List<AuthenticationToken>();
            if (result.Properties?.GetTokenValue("access_token") is string token)
            {
                tokens.Add(new AuthenticationToken { Name = "access_token", Value = token });
            }
            props.StoreTokens(tokens);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                result.Principal,
                props);

            return Redirect("/api/auth/success");
        }

        /// <summary>
        /// Simple success endpoint to verify login and return claims (for testing)
        /// </summary>
        [HttpGet("success")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult Success()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Ok(new
            {
                Message = "Login successful via OpenIddict",
                Subject = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Email = User.FindFirstValue(ClaimTypes.Email),
                Claims = claims
            });
        }

        /// <summary>
        /// Optional: Logout endpoint
        /// </summary>
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Optionally redirect to OpenIddict logout endpoint for SSO logout
            return Redirect("/");
        }
    }
}
