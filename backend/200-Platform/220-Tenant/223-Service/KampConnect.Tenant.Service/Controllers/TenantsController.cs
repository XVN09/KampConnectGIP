using KampConnect.Shared.Contracts.Authorization;
using KampConnect.Tenant.Business.Contracts.ClassTypes;
using KampConnect.Tenant.Business.Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KampConnect.Tenant.Service.Controllers;

[ApiController]
[Route("api/tenants")]

public class TenantsController(ITenantService tenantService) : ControllerBase
{
    [Authorize(Policy = AuthorizationConstants.RequiresTenantPolicy)]
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentTenant(CancellationToken ct)
    {
        // Check if the tenant Id is int and if not, return not found
        if (!int.TryParse(User.FindFirst(KampConnectClaimTypes.TenantId)?.Value, out int userTenantId))
        {
            return NotFound();
        }

        // Get the tenant from the service using the tenant Id
        TenantResult tenantResult = await tenantService.GetCurrentAsync(userTenantId, ct);

        if (tenantResult.Error == TenantError.NotFound)
        {
            return NotFound();
        }

        //Return the tenant 
        return Ok(tenantResult.Tenant);
    }
}
