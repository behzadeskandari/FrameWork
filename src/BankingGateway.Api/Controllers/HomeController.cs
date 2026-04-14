using Microsoft.AspNetCore;
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


        [HttpGet("callback"), HttpPost("callback")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Callback()
        {
            // If this is null, the request didn't come from the Auth Server or was invalid.
            var response = HttpContext.GetOpenIddictClientResponse();
            if (response is null)
            {
                return BadRequest("The OIDC response was not found. Ensure your Redirect URI is correct.");
            }

            if (!string.IsNullOrEmpty(response.Error))
            {
                return BadRequest(new { error = response.Error, description = response.ErrorDescription });
            }

            // 2. Authenticate against the OpenIddict scheme to exchange the code for tokens.
            // OpenIddict handles the heavy lifting here.
            var result = await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal is null)
            {
                return BadRequest("Authentication failed during the code exchange.");
            }

            // 3. Sign in to your local Cookie scheme (BankingGateway session).
            // This bridges the Identity Server session to your local API/Gateway session.
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                result.Principal,
                new AuthenticationProperties { IsPersistent = true });

            return Redirect("/");
        }

        [HttpGet("login")]
        public IActionResult Login(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties { RedirectUri = returnUrl };
            // Trigger the challenge for OpenIddict
            return Challenge(properties, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
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
