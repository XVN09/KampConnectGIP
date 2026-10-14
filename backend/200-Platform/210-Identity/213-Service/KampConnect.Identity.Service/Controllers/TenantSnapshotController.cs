using KampConnect.Identity.Business.Contracts.Interfaces;
using KampConnect.Shared.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace KampConnect.Identity.Service.Controllers;

[ApiController]
[Route("api/me")]
public class TenantSnapshotController(ITenantMembershipService tenantMembershipService) : ControllerBase
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("tenants")]
    public async Task<IActionResult> GetMyTenants(CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirst(KampConnectClaimTypes.Subject)?.Value, out int userId))
        {
            Console.WriteLine("User ID claim is missing or invalid for subject:" + User.FindFirst(KampConnectClaimTypes.Subject)?.Value);
            return Unauthorized();
        }

        var tenants = await tenantMembershipService.GetTenantsForUserAsync(userId, ct);

        if (!tenants.Succeeded)
        {
            return BadRequest(tenants.Errors);
        }
        else
        {
            return Ok(tenants.Tenants);
        }
    }

}
