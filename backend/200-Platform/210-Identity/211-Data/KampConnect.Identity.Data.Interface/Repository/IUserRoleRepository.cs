
using KampConnect.Identity.Data.Entity.Roles;

namespace KampConnect.Identity.Data.Interface.Repository;

public interface IUserRoleRepository
{
    Task<Role?> GetRoleByCodeAsync(string roleCode, CancellationToken ct = default);

    Task<bool> RoleExistsAsync(int userId, int roleId, int tenantId, CancellationToken ct = default);

    public void Add(UserRole userRole);
}
