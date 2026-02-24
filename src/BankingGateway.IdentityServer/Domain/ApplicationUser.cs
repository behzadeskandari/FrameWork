#nullable enable

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BankingGateway.IdentityServer.Domain;

/// <summary>
/// Banking-specific application user extending ASP.NET Core Identity.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? NationalId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastLoginAt { get; set; }

    public int FailedLoginCount { get; set; }

    /// <summary>Banking-grade lockout — explicit until timestamp.</summary>
    public DateTimeOffset? LockedUntil { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    public bool MustChangePassword { get; set; }
}
