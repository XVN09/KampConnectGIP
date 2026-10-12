using KampConnect.Tenant.Data.Context.Implentation;
using KampConnect.Tenant.Data.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.Tenant.Data.Repository.Implentation;

using Tenant = KampConnect.Tenant.Data.Entity.Core.Tenant;

public class TenantRepository(KampConnectTenantDbContext dbContext) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(int tenantId, CancellationToken ct = default) =>
        await dbContext.Tenants.FirstOrDefaultAsync(t => t.tenant_id == tenantId, ct);

    public void Add(Tenant tenant) => dbContext.Tenants.Add(tenant);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) => await dbContext.Tenants.AnyAsync(t => t.tenant_slug == slug, ct);

    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default) => await dbContext.Tenants.AnyAsync(t => t.tenant_name == name, ct);
}

