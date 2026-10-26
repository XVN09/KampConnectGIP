using System;
using KampConnect.Identity.Business.Contracts.ClassTypes;

namespace KampConnect.Identity.Business.Contracts.Interfaces;

public interface ITenantMembershipService
{
    Task<MembershipResult> AddOwnerAsync(int tenantId, Guid userId, CancellationToken ct = default);

    Task<TenantsResult> GetTenantsForUserAsync(Guid userId, CancellationToken ct = default);

    Task<TenantsResult> AddTenant(int tenantId, string tenantName, string tenantSlug, CancellationToken ct = default);
}
