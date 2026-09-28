using KampConnect.Identity.Business.Contracts.ClassTypes;

namespace KampConnect.Identity.Business.Contracts.Interfaces;

/// <summary>Controleert inloggegevens (gebruikt door de loginpagina /oauth2/login).</summary>
public interface ILoginService
{
    /// <summary>
    /// Controleert e-mail + wachtwoord en past de lockout-regels toe.
    /// Elke poging (geslaagd of niet) wordt gelogd in users.sec_login_attempt.
    /// </summary>
    /// <param name="ipAddress">IP van de client, enkel voor de log (mag null zijn).</param>
    Task<LoginResult> ValidateCredentialsAsync(string email, string password, string? ipAddress, CancellationToken ct = default);
}
