using KampConnect.Tenant.Business.Contracts.ClassTypes;

namespace KampConnect.Tenant.Business.Contracts.Interfaces;

public interface ITenantService
{
    Task<TenantResult>GetCurrentAsync(int tenantId, CancellationToken ct = default);

}
