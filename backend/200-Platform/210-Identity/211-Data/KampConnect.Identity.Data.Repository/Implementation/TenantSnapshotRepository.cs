using KampConnect.Identity.Data.Context.Implentation;
using KampConnect.Identity.Data.Entity.Tenant;
using KampConnect.Identity.Data.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.Identity.Data.Repository.Implementation;

public class TenantSnapshotRepository(KampConnectIdentityDbContext dbContext) : ITenantSnapshotRepository
{
    public Task<List<TenantSnapshot>> GetTenantsByIdAsync(List<int> tenantIds, CancellationToken ct = default) =>
        dbContext.TenantSnapshots.Where(t => tenantIds.Contains(t.tenant_id)).ToListAsync(ct);

    public async Task<TenantSnapshot?> AddAsync(TenantSnapshot tenantSnapshot, CancellationToken ct = default) =>
        dbContext.TenantSnapshots.Add(tenantSnapshot).Entity;
}
