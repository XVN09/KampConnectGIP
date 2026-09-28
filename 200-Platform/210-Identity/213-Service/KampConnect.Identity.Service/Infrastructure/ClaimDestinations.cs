using System.Security.Claims;
using KampConnect.Identity.Business.Contracts.Constants;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace KampConnect.Identity.Service.Infrastructure;

/// <summary>
/// Bepaalt per claim in welk token hij terechtkomt.
/// <list type="bullet">
/// <item><b>Access token</b>: voor de API's (de andere services). Hier moeten rollen en tenant_id altijd in.</item>
/// <item><b>Id token</b>: voor de app zelf ("wie is ingelogd?"), afhankelijk van de gevraagde scopes.</item>
/// </list>
/// Belangrijk: OpenIddict neemt een claim ZONDER destination in geen enkel token op.
/// </summary>
public static class ClaimDestinations
{
    /// <summary>Stelt de destinations in voor alle claims van de principal.</summary>
    public static ClaimsPrincipal Apply(ClaimsPrincipal principal)
    {
        principal.SetDestinations(claim => Get(claim, principal));
        return principal;
    }

    private static IEnumerable<string> Get(Claim claim, ClaimsPrincipal principal)
    {
        switch (claim.Type)
        {
            // Wie, voor welke organisatie, beheerder? → altijd in beide tokens
            case KampConnectClaimTypes.Subject:
            case KampConnectClaimTypes.TenantId:
            case KampConnectClaimTypes.IsPlatformAdmin:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                yield break;

            // E-mail enkel als de app de scope "email" vroeg
            case KampConnectClaimTypes.Email:
            case KampConnectClaimTypes.EmailVerified:
                if (principal.HasScope(Scopes.Email))
                {
                    yield return Destinations.AccessToken;
                    yield return Destinations.IdentityToken;
                }
                yield break;

            // Rollen: altijd in het access token (nodig voor [Authorize(Roles = ...)] in de API's),
            // in het id token enkel met scope "roles"
            case KampConnectClaimTypes.Roles:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Roles)) yield return Destinations.IdentityToken;
                yield break;

            // Interne claims van OpenIddict/ASP.NET (bv. security stamp) nooit meesturen
            default:
                yield break;
        }
    }
}
