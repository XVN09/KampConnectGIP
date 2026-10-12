using KampConnect.Tenant.Business.Contracts.ClassTypes;
using KampConnect.Tenant.Business.Contracts.Requests;

namespace KampConnect.Tenant.Business.Contracts.Interfaces;

public interface ITenantService
{
    // Haal huidig tenant op 
    public Task<TenantResult>GetCurrentAsync(int tenantId, CancellationToken ct = default);

    // Maak een nieuwe tenant aan 
    public Task<TenantResult> CreateAsync(TenantCreate request, int userId, CancellationToken ct = default);
}
