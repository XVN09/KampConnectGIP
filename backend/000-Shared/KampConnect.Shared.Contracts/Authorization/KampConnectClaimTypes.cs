namespace KampConnect.Shared.Contracts.Authorization;

/// <summary>
/// Namen van de claims in de tokens van KampConnect (zie api/identity-service.openapi.yaml).
/// <para>
/// Andere services lezen deze claims uit het access token. Gebruik daar dezelfde namen
/// (bv. <c>RoleClaimType = "roles"</c> in AddJwtBearer, zodat [Authorize(Roles = "staff")] werkt).
/// </para>
/// </summary>
public static class KampConnectClaimTypes
{
    /// <summary>Unieke id van de gebruiker (user_id uit users.sec_user) — standaard OIDC-claim.</summary>
    public const string Subject = "sub";

    public const string Email = "email";

    /// <summary>"true"/"false": is het e-mailadres bevestigd?</summary>
    public const string EmailVerified = "email_verified";

    /// <summary>De organisatie (tenant) waarvoor dit token geldt. Ontbreekt bij een platformbeheerder.</summary>
    public const string TenantId = "tenant_id";

    /// <summary>Rolcodes (bv. organisation_admin, staff, guardian). Kan meerdere keren voorkomen.</summary>
    public const string Roles = "roles";

    /// <summary>Enkel aanwezig ("true") voor beheerders van het KampConnect-platform zelf.</summary>
    public const string IsPlatformAdmin = "is_platform_admin";
}
