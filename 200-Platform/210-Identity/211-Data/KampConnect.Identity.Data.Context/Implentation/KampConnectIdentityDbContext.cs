using System;
using System.Collections.Generic;
using System.Text;
using KampConnect.Identity.Data.Entity.Clients;
using KampConnect.Identity.Data.Entity.Claims;
using KampConnect.Identity.Data.Entity.Roles;
using KampConnect.Identity.Data.Entity.Users;
using KampConnect.Identity.Data.Entity.AuthFlow;
using KampConnect.Identity.Data.Entity.PlatformSecurity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace KampConnect.Identity.Data.Context.Implentation;

public class KampConnectIdentityDbContext(
    DbContextOptions<KampConnectIdentityDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    #region DbSets

    // DataProtection API
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    // Clients
    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientParameter> ClientParameters { get; set; }
    public DbSet<ClientValue> ClientValues { get; set; }
    public DbSet<ClientRedirectUri> ClientRedirectUris { get; set; }

    // Claims
    public DbSet<ClientClaim> ClientClaims { get; set; }
    public DbSet<UserClaim> UserClaims { get; set; }
    public DbSet<Claim> Claims { get; set; }

    // Auth Flow
    public DbSet<AuthCode> AuthCodes { get; set; }
    public DbSet<AuthRefreshToken> AuthRefreshTokens { get; set; }

    // Roles
    public DbSet<ClientRole> ClientRoles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    // Users
    public DbSet<User> Users { get; set; }
    public DbSet<UserMfa> UserMfas { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }
    public DbSet<LoginAttempt> LoginAttempts { get; set; }
    public DbSet<ExternalIdentity> ExternalIdentities { get; set; }

    // Platform Security
    public DbSet<SigningKey> SigningKeys { get; set; }
    public DbSet<TenantIdentityProvider> TenantIdentityProviders { get; set; }

    #endregion
}

