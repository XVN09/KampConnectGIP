using KampConnect.Identity.Data.Entity.Claims;
using KampConnect.Identity.Data.Entity.PlatformSecurity;
using KampConnect.Identity.Data.Entity.Roles;
using KampConnect.Identity.Data.Entity.Users;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace KampConnect.Identity.Data.Context.Implentation;

/// <summary>
/// Clients, authorization codes, refresh tokens en signing keys worden beheerd door OpenIddict
/// (eigen entities, geregistreerd in OnModelCreating zodra OpenIddict is toegevoegd).
/// Deze context bevat enkel de eigen tabellen van KampConnect.
/// </summary>
public class KampConnectIdentityDbContext(
    DbContextOptions<KampConnectIdentityDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    #region DbSets

    // DataProtection API
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    // Claims
    public DbSet<Claim> Claims { get; set; }
    public DbSet<UserClaim> UserClaims { get; set; }

    // Roles
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    // Users
    public DbSet<User> Users { get; set; }
    public DbSet<UserMfa> UserMfas { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }
    public DbSet<LoginAttempt> LoginAttempts { get; set; }
    public DbSet<ExternalIdentity> ExternalIdentities { get; set; }

    // Platform Security
    public DbSet<TenantIdentityProvider> TenantIdentityProviders { get; set; }

    #endregion

    #region Model

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseOpenIddict<int>();

        // OpenIddict datatabellen
        builder.Entity<OpenIddictEntityFrameworkCoreApplication<int>>().ToTable("sec_oidc_application", "oidc");
        builder.Entity<OpenIddictEntityFrameworkCoreAuthorization<int>>().ToTable("sec_oidc_authorization", "oidc");
        builder.Entity<OpenIddictEntityFrameworkCoreScope<int>>().ToTable("sec_oidc_scope", "oidc");
        builder.Entity<OpenIddictEntityFrameworkCoreToken<int>>().ToTable("sec_oidc_token", "oidc");

        // Uniek e-mailadres (register → 409 bij dubbel gebruik)
        builder.Entity<User>().HasIndex(u => u.email_normalized).IsUnique();

        // Rolcode komt in de token-claim en moet uniek zijn
        builder.Entity<Role>().HasIndex(r => r.role_code).IsUnique();

        // Een gebruiker heeft een rol maar één keer per tenant
        builder.Entity<UserRole>().HasIndex(ur => new { ur.user_id, ur.role_id, ur.tenant_id }).IsUnique();

        // Opzoeken van tokens (verificatie/reset) gebeurt via de hash
        builder.Entity<UserToken>().HasIndex(t => t.token_hash).IsUnique();

        // Eén externe identiteit per issuer + subject
        builder.Entity<ExternalIdentity>().HasIndex(e => new { e.issuer, e.subject }).IsUnique();
    }

    #endregion
}
