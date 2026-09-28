using KampConnect.Identity.Business.Contracts.ClassTypes;

namespace KampConnect.Identity.Business.Contracts.Interfaces;

/// <summary>Bouwt de claims (sub, email, roles, tenant_id, …) op die in de tokens komen.</summary>
public interface IUserClaimsService
{
    /// <summary>
    /// Laadt de gebruiker met zijn rollen en bouwt een ClaimsIdentity op.
    /// </summary>
    /// <param name="userId">De gebruiker (claim "sub").</param>
    /// <param name="requestedTenantId">
    /// Voor welke organisatie het token moet gelden. Null = automatisch kiezen als de gebruiker
    /// maar bij één organisatie hoort.
    /// </param>
    Task<UserIdentityResult> BuildIdentityAsync(int userId, int? requestedTenantId, CancellationToken ct = default);
}
