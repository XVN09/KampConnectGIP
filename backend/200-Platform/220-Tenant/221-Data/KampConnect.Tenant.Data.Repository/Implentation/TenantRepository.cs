using KampConnect.Tenant.Data.Context.Implentation;
using KampConnect.Tenant.Data.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.Tenant.Data.Repository.Implentation;

using Tenant = KampConnect.Tenant.Data.Entity.Core.Tenant;

public class TenantRepository(KampConnectTenantDbContext dbContext) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(int tenantId, CancellationToken ct = default) =>
        dbContext.Tenants.FirstOrDefaultAsync(t => t.tenant_id == tenantId, ct);
}

