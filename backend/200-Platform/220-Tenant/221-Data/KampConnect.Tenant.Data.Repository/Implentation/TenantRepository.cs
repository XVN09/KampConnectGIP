using KampConnect.Tenant.Data.Context.Implentation;
using KampConnect.Tenant.Data.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.Tenant.Data.Repository.Implentation;

using Tenant = KampConnect.Tenant.Data.Entity.Core.Tenant;

public class TenantRepository(KampConnectTenantDbContext dbContext) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(int tenantId, CancellationToken ct = default) =>
        dbContext.Tenants.FirstOrDefaultAsync(t => t.tenant_id == tenantId, ct);
    public Task<Tenant?> GetBySlugAsync(string tenantSlug, CancellationToken ct = default) =>
        dbContext.Tenants.FirstOrDefaultAsync(t => t.tenant_slug == tenantSlug, ct);
    public Task<bool> SlugExistsAsync(string tenantSlug, CancellationToken ct = default) =>
        dbContext.Tenants.AnyAsync(t => t.tenant_slug == tenantSlug, ct);
    public void Add(Tenant tenant) => dbContext.Tenants.Add(tenant);
    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}

