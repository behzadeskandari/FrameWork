#nullable enable

using BankingGateway.IdentityServer.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BankingGateway.IdentityServer.Data;

/// <summary>
/// Application DbContext combining ASP.NET Core Identity and OpenIddict entity sets.
/// </summary>
public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Register OpenIddict entity sets
        builder.UseOpenIddict<Guid>();

        // ── ApplicationUser ────────────────────────────────────────────
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("IdentityUsers");

            entity.HasIndex(u => u.NationalId)
                  .HasDatabaseName("IX_IdentityUsers_NationalId");

            entity.HasIndex(u => u.Email)
                  .IsUnique()
                  .HasDatabaseName("IX_IdentityUsers_Email");

            entity.Property(u => u.FirstName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(u => u.LastName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(u => u.NationalId)
                  .HasMaxLength(20);

            entity.Property(u => u.Department)
                  .HasMaxLength(100);
        });

        // ── ApplicationRole ────────────────────────────────────────────
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("IdentityRoles");

            entity.Property(r => r.Description)
                  .HasMaxLength(500);
        });

        // ── Restrict cascade deletes on Identity foreign keys ──────────
        // Only restrict Identity-related entities; OpenIddict manages its own cascade behaviour
        var identityEntityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            typeof(ApplicationUser).Name,
            typeof(ApplicationRole).Name,
            "IdentityUserRole",
            "IdentityUserClaim",
            "IdentityUserLogin",
            "IdentityUserToken",
            "IdentityRoleClaim"
        };

        foreach (var relationship in builder.Model.GetEntityTypes()
                     .Where(e => identityEntityNames.Any(n => e.ClrType?.Name.StartsWith(n, StringComparison.OrdinalIgnoreCase) ?? false))
                     .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ApplicationRole>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
