namespace KampConnect.Tenant.Data.Interface.Repository;

using Tenant = KampConnect.Tenant.Data.Entity.Core.Tenant;


public interface ITenantRepository
{

    Task<Tenant?> GetByIdAsync(int tenantId, CancellationToken ct = default);
}
