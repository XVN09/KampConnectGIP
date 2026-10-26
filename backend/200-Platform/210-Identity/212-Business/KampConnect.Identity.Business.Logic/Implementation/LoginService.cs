using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Interfaces;
using KampConnect.Identity.Business.Logic.Helpers;
using KampConnect.Identity.Data.Entity.Users;
using KampConnect.Identity.Data.Interface.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KampConnect.Identity.Business.Logic.Implementation;

/// <summary>
/// Controleert e-mail + wachtwoord.
/// <para>Regels (in deze volgorde):</para>
/// <list type="number">
/// <item>Gebruiker bestaat niet → InvalidCredentials (zelfde antwoord als fout wachtwoord).</item>
/// <item>Account vergrendeld (locked_until in de toekomst) → LockedOut.</item>
/// <item>Account inactief → Inactive.</item>
/// <item>Wachtwoord fout → teller +1; bij het maximum wordt het account vergrendeld.</item>
/// <item>E-mail niet bevestigd (als dat vereist is) → EmailNotVerified.</item>
/// <item>Anders: geslaagd → teller op 0 en last_login invullen.</item>
/// </list>
/// Elke poging wordt gelogd in users.sec_login_attempt, samen met de wijzigingen aan de gebruiker
/// in één SaveChanges (= één transactie).
/// </summary>
public class LoginService(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    IOptions<KampConnectIdentitySettings> options,
    ILogger<LoginService> logger) : ILoginService
{
    private readonly KampConnectIdentitySettings _settings = options.Value;

    // Een geldige hash van een willekeurig wachtwoord. Wordt gebruikt wanneer de gebruiker
    // niet bestaat, zodat de login even lang duurt als bij een bestaande gebruiker.
    // Anders kan een aanvaller aan de reactietijd zien welke e-mailadressen bestaan.
    private static readonly string DummyHash =
        new PasswordHasher<User>().HashPassword(null!, Guid.NewGuid().ToString());

    public async Task<LoginResult> ValidateCredentialsAsync(string email, string password, string? ipAddress, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var user = string.IsNullOrWhiteSpace(email)
            ? null
            : await userRepository.GetByNormalizedEmailAsync(EmailNormalizer.Normalize(email), ct);

        // 1. Onbekende gebruiker
        if (user is null)
        {
            passwordHasher.VerifyHashedPassword(null!, DummyHash, password ?? string.Empty); // zie DummyHash
            return await FailAsync(null, email, ipAddress, LoginFailure.InvalidCredentials, "unknown_user", now, ct);
        }

        // 2. Vergrendeld? (controleren vóór het wachtwoord, anders kan je blijven proberen)
        if (user.locked_until is { } lockedUntil && lockedUntil > now)
        {
            return await FailAsync(user, email, ipAddress, LoginFailure.LockedOut, "locked_out", now, ct, lockedUntil);
        }

        // 3. Gedeactiveerd account
        if (!user.is_active)
        {
            return await FailAsync(user, email, ipAddress, LoginFailure.Inactive, "inactive", now, ct);
        }

        // 4. Wachtwoord controleren
        var verification = user.password_hash is null
            ? PasswordVerificationResult.Failed // bv. account dat enkel via externe login werkt
            : passwordHasher.VerifyHashedPassword(user, user.password_hash, password ?? string.Empty);

        if (verification == PasswordVerificationResult.Failed)
        {
            user.failed_login_count++;
            DateTime? newLock = null;

            if (user.failed_login_count >= _settings.MaxFailedLoginAttempts)
            {
                // Vergrendelen en de teller opnieuw laten beginnen voor na de vergrendeling.
                newLock = now.AddMinutes(_settings.LockoutMinutes);
                user.locked_until = newLock;
                user.failed_login_count = 0;
                logger.LogWarning("Gebruiker {UserId} vergrendeld tot {LockedUntil} na te veel mislukte pogingen", user.user_id, newLock);
            }

            user.modification_timestamp = now;
            return newLock is null
                ? await FailAsync(user, email, ipAddress, LoginFailure.InvalidCredentials, "invalid_password", now, ct)
                : await FailAsync(user, email, ipAddress, LoginFailure.LockedOut, "locked_out", now, ct, newLock);
        }

        // 5. Wachtwoord klopt, maar is de hash met een verouderd algoritme gemaakt? → opnieuw hashen.
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.password_hash = passwordHasher.HashPassword(user, password!);
        }

        // 6. E-mail bevestigd? (na de wachtwoordcontrole, zodat dit niets verklapt aan een aanvaller)
        if (_settings.RequireVerifiedEmail && user.email_verified_timestamp is null)
        {
            return await FailAsync(user, email, ipAddress, LoginFailure.EmailNotVerified, "email_not_verified", now, ct);
        }

        // 7. Geslaagd
        user.failed_login_count = 0;
        user.locked_until = null;
        user.last_login = now;
        user.modification_timestamp = now;

        userRepository.AddLoginAttempt(NewAttempt(user.user_id, email, ipAddress, true, null, now));
        await userRepository.SaveChangesAsync(ct);

        logger.LogInformation("Gebruiker {UserId} ingelogd", user.user_id);
        return LoginResult.Success(user.user_id);
    }

    /// <summary>Logt de mislukte poging, bewaart alles en geeft het resultaat terug.</summary>
    private async Task<LoginResult> FailAsync(
        User? user, string? email, string? ipAddress, LoginFailure failure, string reason,
        DateTime now, CancellationToken ct, DateTime? lockedUntil = null)
    {
        userRepository.AddLoginAttempt(NewAttempt(user?.user_id, email, ipAddress, false, reason, now));
        await userRepository.SaveChangesAsync(ct);

        logger.LogInformation("Login mislukt ({Reason}) voor gebruiker {UserId}", reason, user?.user_id);
        return LoginResult.Fail(failure, lockedUntil);
    }

    private static LoginAttempt NewAttempt(Guid? userId, string? email, string? ip, bool success, string? reason, DateTime now) => new()
    {
        user_id = userId,
        attempted_username = Truncate(email?.Trim(), 100), // kolom is max. 100 tekens
        ip_address = Truncate(ip, 45),                      // IPv6 = max. 45 tekens
        is_successful = success,
        failure_reason = reason,
        creation_timestamp = now
    };

    private static string? Truncate(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];
}
