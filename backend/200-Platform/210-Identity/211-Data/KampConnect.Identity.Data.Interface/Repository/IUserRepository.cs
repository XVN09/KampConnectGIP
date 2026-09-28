using KampConnect.Identity.Data.Entity.Users;

namespace KampConnect.Identity.Data.Interface.Repository;

/// <summary>
/// Toegang tot gebruikers (tabel users.sec_user) en hun inlogpogingen.
/// <para>
/// Deze interface bevat enkel wat de businesslaag nodig heeft. Business.Logic kent alleen
/// deze interface, nooit de implementatie (UserRepository) — zo blijft de logica los van EF Core.
/// </para>
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Haalt een gebruiker op via zijn id. <b>Tracked</b>: wijzigingen worden bewaard bij SaveChangesAsync.
    /// </summary>
    Task<User?> GetByIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Haalt een gebruiker op via zijn genormaliseerd e-mailadres (HOOFDLETTERS, zonder spaties).
    /// <b>Tracked</b>: de login past bv. failed_login_count en locked_until aan.
    /// </summary>
    Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken ct = default);

    /// <summary>
    /// Haalt een gebruiker op mét zijn rollen (UserRoles → Role). <b>Read-only</b> (AsNoTracking):
    /// wordt enkel gebruikt om de claims van het token op te bouwen.
    /// </summary>
    Task<User?> GetByIdWithRolesAsync(int userId, CancellationToken ct = default);

    /// <summary>Bestaat er al een account met dit genormaliseerd e-mailadres?</summary>
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct = default);

    /// <summary>Voegt een nieuwe gebruiker toe (pas in de database na SaveChangesAsync).</summary>
    void Add(User user);

    /// <summary>Logt een inlogpoging (pas in de database na SaveChangesAsync).</summary>
    void AddLoginAttempt(LoginAttempt attempt);

    /// <summary>
    /// Bewaart alle openstaande wijzigingen in één transactie.
    /// Alle repositories delen dezelfde (scoped) DbContext, dus dit bewaart ook wijzigingen
    /// die via een andere repository gebeurden binnen hetzelfde request.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
