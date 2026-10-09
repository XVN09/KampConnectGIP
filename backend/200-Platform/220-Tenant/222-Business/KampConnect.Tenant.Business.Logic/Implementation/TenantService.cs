using KampConnect.Tenant.Business.Contracts.ClassTypes;
using KampConnect.Tenant.Business.Contracts.Interfaces;
using KampConnect.Tenant.Business.Logic.Mapping;
using KampConnect.Tenant.Data.Interface.Repository;

namespace KampConnect.Tenant.Business.Logic.Implementation;

public class TenantService(ITenantRepository repository) : ITenantService
{
    public async Task<TenantResult> GetCurrentAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await repository.GetByIdAsync(tenantId, ct);
        if (tenant is null)
        {
            return TenantResult.Fail(TenantError.NotFound);
        }
        return TenantResult.Ok(tenant: tenant.ToDto());
    }
}
