using KampConnect.Shared.Contracts.Authorization;
using KampConnect.Tenant.Business.Contracts.ClassTypes;
using KampConnect.Tenant.Business.Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KampConnect.Tenant.Business.Contracts.Requests;

namespace KampConnect.Tenant.Service.Controllers;

[ApiController]
[Route("api/tenants")]
public class TenantsController(ITenantService tenantService) : ControllerBase
{
    [Authorize(Policy = AuthorizationConstants.RequiresTenantPolicy)]
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentTenant(CancellationToken ct)
    {
        // Check of tenant id integer is
        if (!int.TryParse(User.FindFirst(KampConnectClaimTypes.TenantId)?.Value, out int userTenantId))
        {
            return NotFound();
        }

        // Haal de tenant op van de service met behulp van de tenant Id
        TenantResult tenantResult = await tenantService.GetCurrentAsync(userTenantId, ct);

        if (tenantResult.Error == TenantError.NotFound)
        {
            return NotFound();
        }

        // Return de tenant 
        return Ok(tenantResult.Tenant);
    }

    [Authorize(Policy = AuthorizationConstants.RequiresAuthenticationPolicy)]
    [HttpPost("create")]
    public async Task<IActionResult> CreateTenant(TenantCreate request, CancellationToken ct)
    {
        int userId = int.Parse(User.FindFirst(KampConnectClaimTypes.Subject)?.Value ?? "0");

        TenantResult tenantResult = await tenantService.CreateAsync(request, userId, ct);

        if (tenantResult.Error == TenantError.SlugInUse)
        {
            return Conflict(new { message = TenantError.SlugInUse });
        }
        if (tenantResult.Error == TenantError.NameAlreadyExists)
        {
            return Conflict(new { message = TenantError.NameAlreadyExists });
        }

        return CreatedAtAction(nameof(GetCurrentTenant), new { id = tenantResult.Tenant.TenantId }, tenantResult.Tenant);
    }
}
