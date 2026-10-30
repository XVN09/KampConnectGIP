using KampConnect.Identity.Data.Entity.Tenant.Invitations;

namespace KampConnect.Identity.Data.Interface.Repository;

public interface ITenantInvitationRepository
{
    Task<List<TenantInvitation>> GetInvitationsByTenantIdAsync(int tenantId, CancellationToken ct = default);

    Task<TenantInvitation?> GetInvitationByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    Task<TenantInvitation?> GetInvitationByIdAsync(int invitationId, CancellationToken ct = default);

    Task<bool> PendingInvitationExistsAsync(int tenantId, string email, CancellationToken ct = default);

    public void Add(TenantInvitation invitation);
}
