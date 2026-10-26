using System;
using KampConnect.Identity.Data.Entity.Users;

namespace KampConnect.Identity.Data.Interface.Repository;

/// <summary>
/// Toegang tot eenmalige tokens (tabel users.sec_user_token) voor e-mailverificatie en wachtwoordherstel.
/// <para>
/// In de database staat nooit het token zelf, enkel een SHA-256-hash ervan. Wie de database
/// kan lezen, kan de tokens dus niet gebruiken.
/// </para>
/// </summary>
public interface IUserTokenRepository
{
    /// <summary>
    /// Zoekt een token dat nog bruikbaar is: juiste hash en doel, niet verlopen en nog niet gebruikt.
    /// De gebruiker wordt mee ingeladen. <b>Tracked</b>, zodat het token als gebruikt gemarkeerd kan worden.
    /// </summary>
    Task<UserToken?> GetUsableAsync(string tokenHash, string purpose, DateTime utcNow, CancellationToken ct = default);

    /// <summary>
    /// Alle nog bruikbare tokens van een gebruiker voor een bepaald doel. <b>Tracked.</b>
    /// Wordt gebruikt om oude tokens ongeldig te maken wanneer er een nieuw verstuurd wordt.
    /// </summary>
    Task<List<UserToken>> GetUsableForUserAsync(Guid userId, string purpose, DateTime utcNow, CancellationToken ct = default);

    /// <summary>Voegt een nieuw token toe (pas in de database na SaveChangesAsync van een repository).</summary>
    void Add(UserToken token);
}
