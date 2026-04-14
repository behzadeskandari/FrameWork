#nullable enable

using BankingGateway.IdentityServer.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BankingGateway.IdentityServer.Controllers;

/// <summary>
/// Handles user login and logout for the consent / login UI.
/// </summary>
public sealed class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    // ── GET /account/login ─────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }



    // ── POST /account/login ────────────────────────────────────────────

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User '{Email}' logged in successfully.", model.Email);

            // FIX: Allow the OIDC authorize path even if IsLocalUrl fails due to absolute paths
            if (!string.IsNullOrEmpty(returnUrl) &&
               (Url.IsLocalUrl(returnUrl) || returnUrl.Contains("/connect/authorize")))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User '{Email}' account locked out.", model.Email);
            ModelState.AddModelError(string.Empty, "Your account has been locked out due to multiple failed attempts.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
        }

        return View(model);
    }


    [HttpPost("activate/{userId}")]
    public async Task<IActionResult> ActivateUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        return Ok();
    }


    // ── POST /account/logout ───────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User signed out.");
        return RedirectToAction("Index", "Home");
    }
}

/// <summary>
/// View model for the login form.
/// </summary>
public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}