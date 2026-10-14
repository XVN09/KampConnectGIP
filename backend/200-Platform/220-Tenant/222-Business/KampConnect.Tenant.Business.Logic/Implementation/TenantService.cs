using KampConnect.Tenant.Business.Contracts.ClassTypes;
using KampConnect.Tenant.Business.Contracts.Interfaces;
using KampConnect.Tenant.Business.Contracts.Requests;
using KampConnect.Tenant.Business.Logic.Helpers;
using KampConnect.Tenant.Business.Logic.Mapping;
using KampConnect.Tenant.Data.Interface.Repository;
using KampConnect.Tenant.Data.Repository.Implentation;
using Microsoft.Extensions.Logging;
using MassTransit;
using KampConnect.Shared.Messaging.Contracts.Tenant;

namespace KampConnect.Tenant.Business.Logic.Implementation;

using Tenant = KampConnect.Tenant.Data.Entity.Core.Tenant;

public class TenantService(ITenantRepository tenantRepository, ILogger<TenantService> logger, IPublishEndpoint publishEndpoint) : ITenantService
{
    #region GetCurrentAsync
    public async Task<TenantResult> GetCurrentAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await tenantRepository.GetByIdAsync(tenantId, ct);
        if (tenant is null)
        {
            return TenantResult.Fail(TenantError.NotFound);
        }
        return TenantResult.Ok(tenant: tenant.ToDto());
    }
    #endregion

    #region CreateAsync
    public async Task<TenantResult> CreateAsync(TenantCreate request, int userId, CancellationToken ct = default)
    {
        // Zet validator op met slug en naam regels
        var validator = new TenantValidator()
            .Slug(request.Slug)
            .Name(request.Name);

        // Gebruik validator om de request te valideren
        if (!validator.IsValid) return TenantResult.Invalid(validator.Errors);

        // Controleer of de slug al bestaat in de repository
        if (await tenantRepository.SlugExistsAsync(request.Slug, ct))
        {
            return TenantResult.Fail(TenantError.SlugInUse);
        }

        // Controleer of de naam al bestaat in de repository
        if (await tenantRepository.NameExistsAsync(request.Name, ct))
        {
            return TenantResult.Fail(TenantError.NameAlreadyExists);
        }

        var now = DateTime.UtcNow;

        // Maak nieuwe tenant aan
        var tenant = new Tenant
        {
            tenant_name = request.Name,
            tenant_slug = request.Slug,
            primary_email = request.PrimaryEmail,
            primary_phone = request.PrimaryPhone,
            tenant_status = TenantStatus.Active.ToString(),
            created_at = now
        };

        // Voeg toe aan repository
        tenantRepository.Add(tenant);

        // Sla op in database
        await tenantRepository.SaveChangesAsync(ct);

        await publishEndpoint.Publish(new TenantCreated
        {
            TenantId = tenant.tenant_id,
            CreatedByUserId = userId,
            TenantName = tenant.tenant_name,
            TenantSlug = tenant.tenant_slug
        }, ct);

        logger.LogInformation("Tenant created with ID {TenantId} and Slug {TenantSlug}", tenant.tenant_id, tenant.tenant_slug);

        // Return de tenant result met de nieuwe tenant id en dto
        return TenantResult.Ok(tenantId: tenant.tenant_id, tenant: tenant.ToDto());
    }
    #endregion
}
