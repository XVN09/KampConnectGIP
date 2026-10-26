
using System;
using KampConnect.Identity.Data.Entity.Roles;

namespace KampConnect.Identity.Data.Interface.Repository;

public interface IUserRoleRepository
{
    Task<List<int>> GetTenantIdsByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Role?> GetRoleByCodeAsync(string roleCode, CancellationToken ct = default);

    Task<bool> RoleExistsAsync(Guid userId, int roleId, int tenantId, CancellationToken ct = default);

    public void Add(UserRole userRole);
}
