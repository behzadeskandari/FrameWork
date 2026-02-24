#nullable enable

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BankingGateway.IdentityServer.Domain;

/// <summary>
/// Banking-specific application role extending ASP.NET Core Identity.
/// </summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsSystemRole { get; set; }
}
