namespace KampConnect.Identity.Business.Contracts.ClassTypes;

/// <summary>
/// Instellingen van de Identity-service (sectie "Identity" in appsettings.json).
/// Wordt in Program.cs gekoppeld via services.Configure&lt;KampConnectIdentitySettings&gt;(...)
/// en in de logica gebruikt via IOptions&lt;KampConnectIdentitySettings&gt;.
/// <para>
/// Bewust niet "IdentityOptions" genoemd: die naam bestaat al in Microsoft.AspNetCore.Identity.
/// </para>
/// </summary>
public class KampConnectIdentitySettings
{
    public const string SectionName = "Identity";

    /// <summary>
    /// Publieke URL van de authorization server (bv. https://auth.kampconnect.be).
    /// Leeg laten = OpenIddict leidt hem af uit het request (handig lokaal).
    /// Instellen zodra de service achter de gateway draait, anders klopt "iss" in de tokens niet.
    /// </summary>
    public string? Issuer { get; set; }

    /// <summary>
    /// Audience ("aud") die in elk access token komt. De andere services controleren deze waarde,
    /// zodat een token van een andere uitgever niet aanvaard wordt.
    /// </summary>
    public string ApiAudience { get; set; } = "kampconnect-api";

    /// <summary>Moet het e-mailadres bevestigd zijn voor je kan inloggen?</summary>
    public bool RequireVerifiedEmail { get; set; } = true;

    /// <summary>Aantal mislukte pogingen na elkaar vóór het account tijdelijk vergrendeld wordt.</summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>Hoelang een account vergrendeld blijft (minuten).</summary>
    public int LockoutMinutes { get; set; } = 15;

    /// <summary>Minimale wachtwoordlengte (spec: 12).</summary>
    public int MinimumPasswordLength { get; set; } = 12;

    /// <summary>Geldigheid van de link in de verificatiemail (uren).</summary>
    public int EmailVerificationLifetimeHours { get; set; } = 24;

    /// <summary>Geldigheid van de link in de herstelmail (minuten).</summary>
    public int PasswordResetLifetimeMinutes { get; set; } = 60;

    /// <summary>
    /// Link in de verificatiemail. {0} wordt vervangen door het token.
    /// De frontend (web/app) opent deze link en roept POST /api/account/verify-email aan.
    /// </summary>
    public string EmailVerificationUrl { get; set; } = "https://kampconnect.be/verify-email?token={0}";

    /// <summary>Link in de herstelmail. {0} wordt vervangen door het token.</summary>
    public string PasswordResetUrl { get; set; } = "https://kampconnect.be/reset-password?token={0}";
}
