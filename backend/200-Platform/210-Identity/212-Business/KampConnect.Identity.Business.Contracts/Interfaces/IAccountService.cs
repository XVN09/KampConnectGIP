using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Requests;

namespace KampConnect.Identity.Business.Contracts.Interfaces;

/// <summary>Registratie, e-mailverificatie en wachtwoordherstel (/api/account/...).</summary>
public interface IAccountService
{
    /// <summary>Maakt een login aan en verstuurt een verificatiemail.</summary>
    Task<AccountResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>Bevestigt het e-mailadres met het token uit de verificatiemail.</summary>
    Task<AccountResult> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);

    /// <summary>
    /// Verstuurt een herstelmail als het account bestaat. Geeft altijd "ok" terug,
    /// zodat je niet kan uitzoeken welke e-mailadressen een account hebben.
    /// </summary>
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);

    /// <summary>Stelt een nieuw wachtwoord in en trekt alle bestaande sessies in.</summary>
    Task<AccountResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
}
