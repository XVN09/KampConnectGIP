using System;

namespace KampConnect.Identity.Business.Contracts.Interfaces;

/// <summary>
/// Trekt alle sessies (refresh tokens en autorisaties) van een gebruiker in.
/// <para>
/// De tokens worden door OpenIddict beheerd. Omdat Business.Logic niets van OpenIddict mag weten,
/// zit de implementatie in de Service-laag (OpenIddictSessionRevoker).
/// </para>
/// </summary>
public interface ISessionRevoker
{
    /// <returns>Aantal ingetrokken tokens/autorisaties.</returns>
    Task<int> RevokeAllAsync(Guid userId, CancellationToken ct = default);
}
