using KampConnect.Identity.ClientRegistration.ClassTypes;
using KampConnect.Identity.Data.Context.Implentation;
using KampConnect.Identity.Data.Entity.Roles;
using KampConnect.Identity.Data.Entity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace KampConnect.Identity.ClientRegistration;

/// <summary>
/// Registreert (idempotent) de OpenIddict-clients en -scopes, de rollen en — enkel in
/// Development — een testgebruiker. Bestaande records worden bijgewerkt, niet gedupliceerd,
/// dus de app mag zo vaak als nodig opnieuw gedraaid worden.
/// </summary>
internal class ClientRegistrationMainService(
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictScopeManager scopeManager,
    KampConnectIdentityDbContext dbContext,
    IOptions<RegistrationSettings> options,
    IHostEnvironment environment,
    ILogger<ClientRegistrationMainService> logger)
{
    #region Declarations

    private readonly RegistrationSettings _settings = options.Value;

    #endregion

    #region Public Methods

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Start registratie ({Environment})", environment.EnvironmentName);

        if (_settings.ApplyMigrations)
        {
            logger.LogInformation("Migraties toepassen");
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        await RegisterScopesAsync(cancellationToken);
        await RegisterClientsAsync(cancellationToken);
        await RegisterRolesAsync(cancellationToken);
        await RegisterTestUserAsync(cancellationToken);

        logger.LogInformation("Registratie voltooid");
    }

    #endregion

    #region Scopes

    private async Task RegisterScopesAsync(CancellationToken cancellationToken)
    {
        foreach (var (name, scope) in _settings.Scopes)
        {
            var descriptor = new OpenIddictScopeDescriptor
            {
                Name = name,
                DisplayName = scope.DisplayName ?? name
            };
            foreach (var resource in scope.Resources)
            {
                descriptor.Resources.Add(resource);
            }

            var existing = await scopeManager.FindByNameAsync(name, cancellationToken);
            if (existing is null)
            {
                await scopeManager.CreateAsync(descriptor, cancellationToken);
                logger.LogInformation("Scope {Scope} aangemaakt", name);
            }
            else
            {
                await scopeManager.UpdateAsync(existing, descriptor, cancellationToken);
                logger.LogInformation("Scope {Scope} bijgewerkt", name);
            }
        }
    }

    #endregion

    #region Clients

    private async Task RegisterClientsAsync(CancellationToken cancellationToken)
    {
        foreach (var (clientId, client) in _settings.Clients)
        {
            var descriptor = BuildApplicationDescriptor(clientId, client);

            var existing = await applicationManager.FindByClientIdAsync(clientId, cancellationToken);
            if (existing is null)
            {
                await applicationManager.CreateAsync(descriptor, cancellationToken);
                logger.LogInformation("Client {ClientId} aangemaakt", clientId);
            }
            else
            {
                await applicationManager.UpdateAsync(existing, descriptor, cancellationToken);
                logger.LogInformation("Client {ClientId} bijgewerkt", clientId);
            }
        }
    }

    private static OpenIddictApplicationDescriptor BuildApplicationDescriptor(string clientId, ClientSettings client)
    {
        var isConfidential = string.Equals(client.ClientType, ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase);
        if (isConfidential && string.IsNullOrWhiteSpace(client.ClientSecret))
        {
            throw new InvalidOperationException($"Client '{clientId}' is confidential maar heeft geen ClientSecret.");
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            DisplayName = string.IsNullOrWhiteSpace(client.DisplayName) ? clientId : client.DisplayName,
            ClientType = isConfidential ? ClientTypes.Confidential : ClientTypes.Public,
            ClientSecret = isConfidential ? client.ClientSecret : null,
            ApplicationType = string.Equals(client.ApplicationType, ApplicationTypes.Web, StringComparison.OrdinalIgnoreCase)
                ? ApplicationTypes.Web
                : ApplicationTypes.Native,
            ConsentType = ConsentTypes.Implicit // eigen apps: geen toestemmingsscherm
        };

        foreach (var uri in client.RedirectUris)
        {
            descriptor.RedirectUris.Add(new Uri(uri, UriKind.Absolute));
        }
        foreach (var uri in client.PostLogoutRedirectUris)
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(uri, UriKind.Absolute));
        }

        // Endpoints
        descriptor.Permissions.Add(Permissions.Endpoints.Token);
        descriptor.Permissions.Add(Permissions.Endpoints.Revocation);
        descriptor.Permissions.Add(Permissions.Endpoints.EndSession);

        // Grant types
        foreach (var grantType in client.GrantTypes)
        {
            switch (grantType)
            {
                case GrantTypes.AuthorizationCode:
                    descriptor.Permissions.Add(Permissions.Endpoints.Authorization);
                    descriptor.Permissions.Add(Permissions.GrantTypes.AuthorizationCode);
                    descriptor.Permissions.Add(Permissions.ResponseTypes.Code);
                    break;
                case GrantTypes.RefreshToken:
                    descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);
                    break;
                case GrantTypes.ClientCredentials when isConfidential:
                    descriptor.Permissions.Add(Permissions.GrantTypes.ClientCredentials);
                    break;
                default:
                    throw new InvalidOperationException($"Grant type '{grantType}' wordt niet ondersteund voor client '{clientId}'.");
            }
        }

        // Scopes (openid heeft geen permissie nodig)
        foreach (var scope in client.Scopes.Where(s => s != Scopes.OpenId))
        {
            descriptor.Permissions.Add(Permissions.Prefixes.Scope + scope);
        }

        if (client.RequirePkce)
        {
            descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
        }

        return descriptor;
    }

    #endregion

    #region Roles

    private async Task RegisterRolesAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        foreach (var (code, settings) in _settings.Roles)
        {
            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.role_code == code, cancellationToken);
            if (role is null)
            {
                dbContext.Roles.Add(new Role
                {
                    role_code = code,
                    role_name = settings.Name,
                    role_description = settings.Description,
                    is_platform_role = settings.IsPlatformRole,
                    creation_timestamp = now
                });
                logger.LogInformation("Rol {Role} aangemaakt", code);
            }
            else
            {
                role.role_name = settings.Name;
                role.role_description = settings.Description;
                role.is_platform_role = settings.IsPlatformRole;
                role.modification_timestamp = now;
                logger.LogInformation("Rol {Role} bijgewerkt", code);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Test User

    private async Task RegisterTestUserAsync(CancellationToken cancellationToken)
    {
        var testUser = _settings.TestUser;
        if (testUser is null || string.IsNullOrWhiteSpace(testUser.Email))
        {
            return;
        }

        if (!environment.IsDevelopment())
        {
            logger.LogWarning("TestUser genegeerd: enkel toegelaten in Development");
            return;
        }

        var now = DateTime.UtcNow;
        var normalizedEmail = testUser.Email.Trim().ToUpperInvariant();

        var user = await dbContext.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.email_normalized == normalizedEmail, cancellationToken);

        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(testUser.Password))
            {
                throw new InvalidOperationException("TestUser heeft geen Password (zet het via user secrets).");
            }

            user = new User
            {
                username = testUser.Email.Trim(),
                email = testUser.Email.Trim(),
                email_normalized = normalizedEmail,
                email_verified_timestamp = now,
                is_active = true,
                failed_login_count = 0,
                creation_timestamp = now
            };
            user.password_hash = new PasswordHasher<User>().HashPassword(user, testUser.Password);

            dbContext.Users.Add(user);
            logger.LogInformation("Testgebruiker {Email} aangemaakt", testUser.Email);
        }
        else if (!string.IsNullOrWhiteSpace(testUser.Password))
        {
            user.password_hash = new PasswordHasher<User>().HashPassword(user, testUser.Password);
            user.email_verified_timestamp ??= now;
            user.is_active = true;
            user.failed_login_count = 0;
            user.locked_until = null;
            user.modification_timestamp = now;
            logger.LogInformation("Testgebruiker {Email} bestaat al (wachtwoord bijgewerkt)", testUser.Email);
        }
        else
        {
            logger.LogInformation("Testgebruiker {Email} bestaat al (wachtwoord ongewijzigd)", testUser.Email);
        }

        foreach (var roleCode in testUser.Roles)
        {
            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.role_code == roleCode, cancellationToken)
                ?? throw new InvalidOperationException($"Rol '{roleCode}' van de testgebruiker bestaat niet.");

            var tenantId = role.is_platform_role ? null : testUser.TenantId;
            if (!role.is_platform_role && tenantId is null)
            {
                throw new InvalidOperationException($"Rol '{roleCode}' vereist een TenantId voor de testgebruiker.");
            }

            var alreadyLinked = user.UserRoles.Any(ur => ur.role_id == role.role_id && ur.tenant_id == tenantId);
            if (!alreadyLinked)
            {
                user.UserRoles.Add(new UserRole
                {
                    role_id = role.role_id,
                    user_id = user.user_id,
                    tenant_id = tenantId,
                    creation_timestamp = now
                });
                logger.LogInformation("Rol {Role} gekoppeld aan testgebruiker", roleCode);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
