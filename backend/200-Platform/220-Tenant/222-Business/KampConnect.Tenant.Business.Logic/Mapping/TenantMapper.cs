using KampConnect.Tenant.Business.Contracts.ClassTypes;
using KampConnect.Tenant.Business.Logic.Helpers;

namespace KampConnect.Tenant.Business.Logic.Mapping;

using Tenant = KampConnect.Tenant.Data.Entity.Core.Tenant;

internal static class TenantMapper
{

    public static TenantDto ToDto(this Tenant tenant)
    {
        return new TenantDto(TenantId: tenant.tenant_id,
                             Name: tenant.tenant_name,
                             Slug: tenant.tenant_slug,
                             Status: StatusHelper.ParseStatus(tenant.tenant_status),
                             PrimaryEmail: tenant.primary_email,
                             PrimaryPhone: tenant.primary_phone,
                             CreatedAt: tenant.created_at,
                             UpdatedAt: tenant.updated_at);
    }
}

