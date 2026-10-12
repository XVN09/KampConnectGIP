namespace KampConnect.Tenant.Data.Interface.Repository;

using Tenant = KampConnect.Tenant.Data.Entity.Core.Tenant;


public interface ITenantRepository
{

    public Task<Tenant?> GetByIdAsync(int tenantId, CancellationToken ct = default);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);

    public Task<bool> NameExistsAsync(string name, CancellationToken ct = default);

    public void Add(Tenant tenant);

    public Task SaveChangesAsync(CancellationToken ct = default);
}
