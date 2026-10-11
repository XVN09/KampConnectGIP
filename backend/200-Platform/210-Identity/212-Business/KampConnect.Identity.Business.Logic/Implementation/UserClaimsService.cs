using System.Security.Claims;
using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Constants;
using KampConnect.Identity.Business.Contracts.Interfaces;
using KampConnect.Identity.Data.Interface.Repository;
using KampConnect.Shared.Contracts.Authorization;

namespace KampConnect.Identity.Business.Logic.Implementation;

/// <summary>
/// Bouwt de claims van een gebruiker op voor de tokens.
/// <para><b>Hoe de tenant (organisatie) gekozen wordt:</b></para>
/// <list type="bullet">
/// <item>Een rol hoort bij een koppeling (sec_user_role) met een tenant_id — niet bij het account.</item>
/// <item>Platformrollen (is_platform_role) hebben geen tenant.</item>
/// <item>Vraagt de client een tenant_id (parameter in /oauth2/authorize), dan moet de gebruiker daar een rol hebben.</item>
/// <item>Geen tenant gevraagd + maar één organisatie → die wordt automatisch gekozen.</item>
/// <item>Geen tenant gevraagd + meerdere organisaties → fout TenantRequired (de app moet laten kiezen).</item>
/// </list>
/// Het resultaat bevat enkel .NET-claims; de Service-laag bepaalt in welk token (access/id) elke claim komt.
/// </summary>
public class UserClaimsService(IUserRepository userRepository) : IUserClaimsService
{
    public async Task<UserIdentityResult> BuildIdentityAsync(int userId, int? requestedTenantId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdWithRolesAsync(userId, ct);

        // Gebruiker verwijderd of gedeactiveerd sinds het vorige token → geen nieuw token.
        if (user is null || !user.is_active)
        {
            return UserIdentityResult.Fail(UserIdentityFailure.UserUnavailable);
        }

        // Rollen opsplitsen in platformrollen (zonder tenant) en organisatierollen (met tenant).
        var links = user.UserRoles.Where(ur => ur.Role is not null).ToList();
        var platformRoles = links.Where(ur => ur.Role!.is_platform_role).ToList();
        var tenantRoles = links.Where(ur => !ur.Role!.is_platform_role && ur.tenant_id is not null).ToList();
        var tenantIds = tenantRoles.Select(ur => ur.tenant_id!.Value).Distinct().ToList();

        // Tenant kiezen (zie uitleg bovenaan).
        int? tenantId = requestedTenantId;
        if (tenantId is not null && !tenantIds.Contains(tenantId.Value))
        {
            return UserIdentityResult.Fail(UserIdentityFailure.TenantNotAllowed);
        }
        if (tenantId is null)
        {
            if (tenantIds.Count > 1) return UserIdentityResult.Fail(UserIdentityFailure.TenantRequired);
            if (tenantIds.Count == 1) tenantId = tenantIds[0];
        }

        // De identiteit zelf. authenticationType moet ingevuld zijn, anders telt ze als "niet ingelogd".
        var identity = new ClaimsIdentity(
            authenticationType: "KampConnect",
            nameType: KampConnectClaimTypes.Email,
            roleType: KampConnectClaimTypes.Roles);

        identity.AddClaim(new Claim(KampConnectClaimTypes.Subject, user.user_id.ToString()));

        if (!string.IsNullOrEmpty(user.email))
        {
            identity.AddClaim(new Claim(KampConnectClaimTypes.Email, user.email));
        }

        // ClaimValueTypes.Boolean/Integer32 zorgen dat het in de JWT als true/getal staat, niet als "true"/"12".
        identity.AddClaim(new Claim(KampConnectClaimTypes.EmailVerified,
            user.email_verified_timestamp is not null ? "true" : "false", ClaimValueTypes.Boolean));

        if (tenantId is not null)
        {
            identity.AddClaim(new Claim(KampConnectClaimTypes.TenantId, tenantId.Value.ToString(), ClaimValueTypes.Integer32));
        }

        // Rollen: alle platformrollen + de rollen binnen de gekozen tenant. Elke rol = één claim.
        var roleCodes = platformRoles.Select(ur => ur.Role!.role_code)
            .Concat(tenantRoles.Where(ur => ur.tenant_id == tenantId).Select(ur => ur.Role!.role_code))
            .Distinct();
        foreach (var roleCode in roleCodes)
        {
            identity.AddClaim(new Claim(KampConnectClaimTypes.Roles, roleCode));
        }

        if (platformRoles.Count > 0)
        {
            identity.AddClaim(new Claim(KampConnectClaimTypes.IsPlatformAdmin, "true", ClaimValueTypes.Boolean));
        }

        return UserIdentityResult.Success(identity);
    }
}
