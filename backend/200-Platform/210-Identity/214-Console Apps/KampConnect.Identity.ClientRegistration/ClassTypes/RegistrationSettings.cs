namespace KampConnect.Identity.ClientRegistration.ClassTypes;

/// <summary>
/// Sectie "Registration" uit appsettings. Clients, scopes en rollen staan in een dictionary
/// (sleutel = clientId / scopenaam / rolcode) zodat appsettings.{Environment}.json ze kan
/// aanvullen of overschrijven zonder dat arrays per index samengevoegd worden.
/// </summary>
public class RegistrationSettings
{
    public bool ApplyMigrations { get; set; }
    public Dictionary<string, ScopeSettings> Scopes { get; set; } = new();
    public Dictionary<string, ClientSettings> Clients { get; set; } = new();
    public Dictionary<string, RoleSettings> Roles { get; set; } = new();
    public TestUserSettings? TestUser { get; set; }
}
public class ClientSettings
{
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>"public" (app, SPA) of "confidential" (server met secret).</summary>
    public string ClientType { get; set; } = "public";

    /// <summary>Enkel voor confidential clients — bij voorkeur via user secrets.</summary>
    public string? ClientSecret { get; set; }

    /// <summary>"native" (mobiele app) of "web".</summary>
    public string ApplicationType { get; set; } = "native";

    public bool RequirePkce { get; set; } = true;
    public List<string> GrantTypes { get; set; } = new();
    public List<string> RedirectUris { get; set; } = new();
    public List<string> PostLogoutRedirectUris { get; set; } = new();
    public List<string> Scopes { get; set; } = new();
}

public class ScopeSettings
{
    public string? DisplayName { get; set; }

    /// <summary>Audiences (resource servers) die in het access token komen voor deze scope.</summary>
    public List<string> Resources { get; set; } = new();
}

public class RoleSettings
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPlatformRole { get; set; }
}

public class TestUserSettings
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int? TenantId { get; set; }
    public List<string> Roles { get; set; } = new();
}
