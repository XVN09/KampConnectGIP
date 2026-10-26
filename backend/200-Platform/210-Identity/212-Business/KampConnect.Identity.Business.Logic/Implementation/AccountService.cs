using System.Net;
using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Constants;
using KampConnect.Identity.Business.Contracts.Interfaces;
using KampConnect.Identity.Business.Contracts.Requests;
using KampConnect.Identity.Business.Logic.Helpers;
using KampConnect.Identity.Data.Entity.Users;
using KampConnect.Identity.Data.Interface.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KampConnect.Identity.Business.Logic.Implementation;

/// <summary>
/// Registratie, e-mailverificatie en wachtwoordherstel.
/// <para>
/// Tokens in mails zijn eenmalig en tijdelijk: in de database staat enkel hun hash
/// (zie <see cref="SecureToken"/>). Een nieuw token maakt oudere tokens met hetzelfde doel ongeldig.
/// </para>
/// </summary>
public class AccountService(
    IUserRepository userRepository,
    IUserTokenRepository tokenRepository,
    IPasswordHasher<User> passwordHasher,
    IIdentityMailSender mailSender,
    ISessionRevoker sessionRevoker,
    IOptions<KampConnectIdentitySettings> options,
    ILogger<AccountService> logger) : IAccountService
{
    private readonly KampConnectIdentitySettings _settings = options.Value;

    #region Register

    public async Task<AccountResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        // 1. Invoer controleren (veldnamen zoals in de JSON → "email", "password")
        var validator = new AccountValidator()
            .Email("email", request.Email)
            .Password("password", request.Password, _settings.MinimumPasswordLength);
        if (!validator.IsValid) return AccountResult.Invalid(validator.Errors);

        var email = request.Email.Trim();
        var normalized = EmailNormalizer.Normalize(email);

        // 2. E-mail al in gebruik? → 409
        //    (De unieke index op email_normalized vangt ook een gelijktijdige dubbele registratie op.)
        if (await userRepository.EmailExistsAsync(normalized, ct))
        {
            return AccountResult.Fail(AccountError.EmailInUse);
        }

        // 3. Gebruiker aanmaken. Enkel de login: naam, adres, … beheert de User-service.
        var now = DateTime.UtcNow;
        var user = new User
        {
            username = email,
            email = email,
            email_normalized = normalized,
            is_active = true,
            failed_login_count = 0,
            creation_timestamp = now
        };
        user.password_hash = passwordHasher.HashPassword(user, request.Password);
        userRepository.Add(user);

        // 4. Verificatietoken aanmaken (hoort bij de nieuwe gebruiker via de navigatie-property,
        //    EF vult user_id zelf in bij het bewaren).
        var (token, hash) = SecureToken.Create();
        user.UserTokens.Add(new UserToken
        {
            user_id = Guid.Empty, // wordt door EF ingevuld via de relatie
            token_hash = hash,
            token_purpose = UserTokenPurposes.EmailVerification,
            expiration_timestamp = now.AddHours(_settings.EmailVerificationLifetimeHours),
            creation_timestamp = now
        });

        // 5. Alles in één transactie bewaren
        await userRepository.SaveChangesAsync(ct);

        // TODO (Tenant-service): request.InvitationToken doorgeven zodat de rol + organisatie
        //      uit de uitnodiging gekoppeld worden (sec_user_role). Tot dan heeft een nieuw
        //      account nog geen rollen.
        if (!string.IsNullOrWhiteSpace(request.InvitationToken))
        {
            logger.LogInformation("Gebruiker {UserId} registreerde met een uitnodigingstoken (koppeling volgt via Tenant-service)", user.user_id);
        }

        // 6. Verificatiemail versturen (na het bewaren: het token moet bestaan als de gebruiker klikt)
        await SendVerificationMailAsync(email, token, ct);

        logger.LogInformation("Account {UserId} aangemaakt", user.user_id);
        return AccountResult.Ok(user.user_id);
    }

    #endregion

    #region Verify email

    public async Task<AccountResult> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token)) return AccountResult.Fail(AccountError.InvalidToken);

        var now = DateTime.UtcNow;
        var userToken = await tokenRepository.GetUsableAsync(
            SecureToken.Hash(request.Token), UserTokenPurposes.EmailVerification, now, ct);

        if (userToken?.User is null) return AccountResult.Fail(AccountError.InvalidToken);

        // Token opgebruikt + e-mail bevestigd (als dat nog niet zo was)
        userToken.consumed_timestamp = now;
        userToken.User.email_verified_timestamp ??= now;
        userToken.User.modification_timestamp = now;

        await userRepository.SaveChangesAsync(ct);
        logger.LogInformation("E-mail van gebruiker {UserId} bevestigd", userToken.user_id);
        return AccountResult.Ok(userToken.user_id);
    }

    #endregion

    #region Forgot / reset password

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        // Altijd "ok" teruggeven (de controller geeft altijd 204): we verklappen niet of het account bestaat.
        if (string.IsNullOrWhiteSpace(request.Email)) return;

        var user = await userRepository.GetByNormalizedEmailAsync(EmailNormalizer.Normalize(request.Email), ct);
        if (user is null || !user.is_active || string.IsNullOrEmpty(user.email))
        {
            logger.LogInformation("Wachtwoordherstel gevraagd voor onbekend/inactief account");
            return;
        }

        var now = DateTime.UtcNow;

        // Oude, nog geldige herstellinks ongeldig maken: enkel de laatste mail werkt.
        foreach (var old in await tokenRepository.GetUsableForUserAsync(user.user_id, UserTokenPurposes.PasswordReset, now, ct))
        {
            old.consumed_timestamp = now;
        }

        var (token, hash) = SecureToken.Create();
        tokenRepository.Add(new UserToken
        {
            user_id = user.user_id,
            token_hash = hash,
            token_purpose = UserTokenPurposes.PasswordReset,
            expiration_timestamp = now.AddMinutes(_settings.PasswordResetLifetimeMinutes),
            creation_timestamp = now
        });
        await userRepository.SaveChangesAsync(ct);

        var link = string.Format(_settings.PasswordResetUrl, Uri.EscapeDataString(token));
        await mailSender.SendAsync(user.email, "KampConnect – nieuw wachtwoord instellen",
            $"<p>Je vroeg een nieuw wachtwoord aan voor KampConnect.</p>" +
            $"<p><a href=\"{WebUtility.HtmlEncode(link)}\">Stel een nieuw wachtwoord in</a> " +
            $"(geldig tot {_settings.PasswordResetLifetimeMinutes} minuten na deze mail).</p>" +
            $"<p>Heb jij dit niet gevraagd? Dan mag je deze mail negeren.</p>", ct);

        logger.LogInformation("Herstelmail verstuurd naar gebruiker {UserId}", user.user_id);
    }

    public async Task<AccountResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        // 1. Nieuw wachtwoord controleren (veldnaam "newPassword" zoals in de JSON)
        var validator = new AccountValidator()
            .Password("newPassword", request.NewPassword, _settings.MinimumPasswordLength);
        if (!validator.IsValid) return AccountResult.Invalid(validator.Errors);

        // 2. Token opzoeken
        if (string.IsNullOrWhiteSpace(request.Token)) return AccountResult.Fail(AccountError.InvalidToken);
        var now = DateTime.UtcNow;
        var userToken = await tokenRepository.GetUsableAsync(
            SecureToken.Hash(request.Token), UserTokenPurposes.PasswordReset, now, ct);
        if (userToken?.User is null) return AccountResult.Fail(AccountError.InvalidToken);

        // 3. Wachtwoord vervangen en een eventuele vergrendeling opheffen
        var user = userToken.User;
        user.password_hash = passwordHasher.HashPassword(user, request.NewPassword);
        user.failed_login_count = 0;
        user.locked_until = null;
        // Wie de mail kon openen, bewijst ook dat het e-mailadres van hem is.
        user.email_verified_timestamp ??= now;
        user.modification_timestamp = now;
        userToken.consumed_timestamp = now;

        await userRepository.SaveChangesAsync(ct);

        // 4. Alle bestaande sessies intrekken (spec): wie het oude wachtwoord kende, is er nu uit.
        var revoked = await sessionRevoker.RevokeAllAsync(user.user_id, ct);
        logger.LogInformation("Wachtwoord van gebruiker {UserId} gewijzigd, {Count} sessies ingetrokken", user.user_id, revoked);

        return AccountResult.Ok(user.user_id);
    }

    #endregion

    #region Helpers

    private Task SendVerificationMailAsync(string email, string token, CancellationToken ct)
    {
        var link = string.Format(_settings.EmailVerificationUrl, Uri.EscapeDataString(token));
        return mailSender.SendAsync(email, "KampConnect – bevestig je e-mailadres",
            $"<p>Welkom bij KampConnect!</p>" +
            $"<p><a href=\"{WebUtility.HtmlEncode(link)}\">Bevestig je e-mailadres</a> " +
            $"(geldig gedurende {_settings.EmailVerificationLifetimeHours} uur).</p>", ct);
    }

    #endregion
}
