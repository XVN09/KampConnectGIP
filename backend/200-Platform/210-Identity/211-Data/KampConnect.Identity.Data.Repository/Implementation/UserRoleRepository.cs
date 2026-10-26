using System;
using KampConnect.Identity.Data.Context.Implentation;
using KampConnect.Identity.Data.Interface.Repository;
using KampConnect.Identity.Data.Entity.Roles;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.Identity.Data.Repository.Implementation;

public class UserRoleRepository(KampConnectIdentityDbContext dbContext) : IUserRoleRepository
{
    public void Add(UserRole userRole) => dbContext.UserRoles.Add(userRole);

    public Task<Role?> GetRoleByCodeAsync(string roleCode, CancellationToken ct = default) =>
        dbContext.Roles.FirstOrDefaultAsync(r => r.role_code == roleCode, ct);

    public Task<bool> RoleExistsAsync(Guid userId, int roleId, int tenantId, CancellationToken ct = default) =>
        dbContext.UserRoles.AnyAsync(ur =>
            ur.user_id == userId &&
            ur.role_id == roleId &&
            ur.tenant_id == tenantId, ct);

    public Task<List<int>> GetTenantIdsByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        dbContext.UserRoles
        .Where(ur => ur.user_id == userId && ur.tenant_id != null)
        .Select(ur => ur.tenant_id!.Value)
        .Distinct()
        .ToListAsync(ct);
}
