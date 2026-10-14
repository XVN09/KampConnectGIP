using System;
using System.Collections.Generic;
using System.Text;
using KampConnect.Identity.Data.Entity.Tenant;

namespace KampConnect.Identity.Data.Interface.Repository;

public interface ITenantSnapshotRepository
{
    Task<List<TenantSnapshot>> GetTenantsByIdAsync(List<int> tenantIds, CancellationToken ct = default);

    Task<TenantSnapshot?> AddAsync(TenantSnapshot tenantSnapshot, CancellationToken ct = default);
}
