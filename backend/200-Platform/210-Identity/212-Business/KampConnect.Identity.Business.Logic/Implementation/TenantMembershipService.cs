using System;
using System.Collections.Generic;
using System.Text;
using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Constants;
using KampConnect.Identity.Business.Contracts.Dto;
using KampConnect.Identity.Business.Contracts.Interfaces;
using KampConnect.Identity.Data.Entity.Roles;
using KampConnect.Identity.Data.Entity.Tenant;
using KampConnect.Identity.Data.Interface.Repository;
using Microsoft.Extensions.Logging;

namespace KampConnect.Identity.Business.Logic.Implementation;

public class TenantMembershipService(
    IUserRoleRepository userRoleRepository,
    IUserRepository userRepository,
    ITenantSnapshotRepository tenantSnapshotRepository,
    ILogger<TenantMembershipService> logger) : ITenantMembershipService
{
    public async Task<MembershipResult> AddOwnerAsync(int tenantId, int userId, CancellationToken ct = default)
    {
        // Owner rol id ophalen
        const string OwnerRoleCode = RoleCodes.OrganisationAdmin;

        if (OwnerRoleCode == null)
        {
            logger.LogError("Role with code {RoleCode} not found when trying to add user {UserId} as owner to tenant {TenantId}", OwnerRoleCode, userId, tenantId);
            return MembershipResult.Fail(MembershipError.RoleMissing);
        }

        var OwnerRoleId = await userRoleRepository.GetRoleByCodeAsync(OwnerRoleCode, ct);

        if (OwnerRoleId == null)
        {
            logger.LogError("Role with code {RoleCode} not found when trying to add user {UserId} as owner to tenant {TenantId}", OwnerRoleCode, userId, tenantId);
            return MembershipResult.Fail(MembershipError.RoleMissing);
        }

        Console.WriteLine("UserId: " + userId);

        // Controleer of de gebruiker bestaat
        if (await userRepository.GetByIdAsync(userId, ct) == null)
        {
            logger.LogInformation("User {UserId} not found when trying to add as owner to tenant {TenantId}", userId, tenantId);    
            return MembershipResult.Fail(MembershipError.UserUnavailable);
        }

        var now = DateTime.UtcNow;
        var userRole = new UserRole
        {
            user_id = userId,
            tenant_id = tenantId,
            role_id = OwnerRoleId.role_id,
            creation_timestamp = now
        };

        if (await userRoleRepository.RoleExistsAsync(userId, OwnerRoleId.role_id, tenantId, ct))
        {
            logger.LogInformation("User {UserId} already has role {RoleCode} for tenant {TenantId}", userId, OwnerRoleCode, tenantId);
            return MembershipResult.Success();
        }

        userRoleRepository.Add(userRole);

        await userRepository.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} added as owner to tenant {TenantId}", userId, tenantId);

        return MembershipResult.Success();
    }

    public async Task<TenantsResult> GetTenantsForUserAsync(int userId, CancellationToken ct = default)
    {
        List<int> tenantIds = await userRoleRepository.GetTenantIdsByUserIdAsync(userId, ct);

        List<TenantSnapshot> tenants = await tenantSnapshotRepository.GetTenantsByIdAsync(tenantIds, ct);

        List<TenantDto> tenantDtos = tenants.Select(t => new TenantDto
        {
            TenantId = t.tenant_id,
            TenantName = t.tenant_name,
            TenantSlug = t.tenant_slug
        }).ToList();

        return TenantsResult.Success(tenantDtos);
    }

    public async Task<TenantsResult> AddTenant(int tenantId, string tenantName, string tenantSlug, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var tenantSnapshot = new TenantSnapshot
        {
            tenant_id = tenantId,
            tenant_name = tenantName,
            tenant_slug = tenantSlug,
            last_synced_timestamp = now
        };
        await tenantSnapshotRepository.AddAsync(tenantSnapshot, ct);
        await userRepository.SaveChangesAsync(ct);
        logger.LogInformation("Tenant snapshot added with ID {TenantId}, Name {TenantName}, Slug {TenantSlug}", tenantId, tenantName, tenantSlug);
        return TenantsResult.Success(new List<TenantDto> { new TenantDto { TenantId = tenantId, TenantName = tenantName, TenantSlug = tenantSlug } });
    }
}
