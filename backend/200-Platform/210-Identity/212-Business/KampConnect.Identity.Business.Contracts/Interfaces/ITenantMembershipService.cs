using KampConnect.Identity.Business.Contracts.ClassTypes;

namespace KampConnect.Identity.Business.Contracts.Interfaces;

public interface ITenantMembershipService
{
    Task<MembershipResult> AddOwnerAsync(int userId, int tenantId, CancellationToken ct = default);

    Task<TenantsResult> GetTenantsForUserAsync(int userId, CancellationToken ct = default);

    Task<TenantsResult> AddTenant(int tenantId, string tenantName, string tenantSlug, CancellationToken ct = default);
}
