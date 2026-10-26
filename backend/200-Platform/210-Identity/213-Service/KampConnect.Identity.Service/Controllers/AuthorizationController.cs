using System;
using System.Security.Claims;
using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Constants;
using KampConnect.Identity.Business.Contracts.Interfaces;
using KampConnect.Identity.Service.Infrastructure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using KampConnect.Shared.Contracts.Authorization;

namespace KampConnect.Identity.Service.Controllers;

/// <summary>
/// De OAuth 2.1 / OpenID Connect-endpoints waarvoor OpenIddict "passthrough" aanstaat:
/// OpenIddict controleert eerst het request (client, redirect_uri, PKCE, code, …) en geeft het
/// daarna aan deze controller. Hier beslissen WIJ wie de gebruiker is en welke claims hij krijgt.
/// <para>
/// Endpoints die OpenIddict volledig zelf afhandelt (geen code nodig):
/// /.well-known/openid-configuration, /.well-known/jwks.json en /oauth2/revoke.
/// </para>
/// </summary>
public class AuthorizationController(
    IUserClaimsService claimsService,
    IOptions<KampConnectIdentitySettings> options,
    ILogger<AuthorizationController> logger) : Controller
{
    /// <summary>Extra (niet-standaard) parameter waarmee de app kan kiezen voor welke organisatie ze inlogt.</summary>
    private const string TenantParameter = "tenant_id";

    #region /oauth2/authorize

    /// <summary>
    /// Stap 1 van de login-flow (authorization code + PKCE).
    /// <list type="number">
    /// <item>Nog niet ingelogd (geen cookie) → doorsturen naar /oauth2/login. Die stuurt daarna terug naar hier.</item>
    /// <item>Wel ingelogd → claims opbouwen en "SignIn" met OpenIddict. OpenIddict maakt dan een
    /// eenmalige code (60 s geldig) en stuurt de browser terug naar de app (redirect_uri?code=…&amp;state=…).</item>
    /// </list>
    /// </summary>
    [HttpGet("~/oauth2/authorize")]
    [HttpPost("~/oauth2/authorize")]
    [IgnoreAntiforgeryToken] // de app post dit niet met een antiforgery-token (response_mode=form_post)
    public async Task<IActionResult> Authorize(CancellationToken ct)
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("Het OpenID Connect-request kon niet gelezen worden.");

        // Is de gebruiker al ingelogd in deze browser (cookie van de loginpagina)?
        var cookie = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!cookie.Succeeded || cookie.Principal is null)
        {
            // Nee → naar de loginpagina. RedirectUri = dit request (met alle parameters),
            // zodat de login-flow na het inloggen gewoon verdergaat.
            return Challenge(
                authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        // Ja → user_id uit de cookie halen
        if (!Guid.TryParse(cookie.Principal.FindFirst(KampConnectClaimTypes.Subject)?.Value, out var userId))
        {
            // Kapotte of oude cookie: uitloggen en opnieuw laten inloggen
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Challenge(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // Optioneel gevraagde organisatie
        int? requestedTenant = int.TryParse((string?)request[TenantParameter], out var t) ? t : null;

        var result = await claimsService.BuildIdentityAsync(userId, requestedTenant, ct);
        if (!result.Succeeded)
        {
            // Bij /authorize stuurt OpenIddict de fout terug naar de app (redirect_uri?error=…).
            return ForbidWithError(result.Failure == UserIdentityFailure.UserUnavailable ? Errors.AccessDenied : Errors.InvalidRequest,
                                   Describe(result.Failure));
        }

        // Principal maken met de scopes die de app vroeg (bv. openid email roles offline_access)
        var principal = new ClaimsPrincipal(result.Identity!);
        principal.SetScopes(request.GetScopes());
        principal.SetResources(new[] { options.Value.ApiAudience }); // → "aud" in het access token
        ClaimDestinations.Apply(principal);

        logger.LogInformation("Autorisatie voor gebruiker {UserId} via client {ClientId}", userId, request.ClientId);

        // OpenIddict neemt het over: code aanmaken + redirect naar de app
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    #endregion

    #region /oauth2/token

    /// <summary>
    /// Stap 2: de app wisselt de code in voor tokens, of vernieuwt ze met een refresh token.
    /// <para>
    /// OpenIddict heeft de code/refresh token + PKCE (code_verifier) al gecontroleerd.
    /// Wij laden de gebruiker opnieuw uit de database: is hij intussen gedeactiveerd of zijn zijn
    /// rollen gewijzigd, dan zit dat meteen in het nieuwe token.
    /// </para>
    /// </summary>
    [HttpPost("~/oauth2/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange(CancellationToken ct)
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("Het OpenID Connect-request kon niet gelezen worden.");

        if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
        {
            // Andere grant types (password, client_credentials, …) staan niet aan in Program.cs,
            // dus dit zou nooit mogen gebeuren.
            throw new InvalidOperationException("Dit grant type wordt niet ondersteund.");
        }

        // De principal die bij de code / het refresh token hoort (door OpenIddict uitgelezen)
        var stored = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        if (stored is null || !Guid.TryParse(stored.GetClaim(Claims.Subject), out var userId))
        {
            return ForbidWithError(Errors.InvalidGrant, "Het token is niet meer geldig.");
        }

        // Dezelfde organisatie als bij het inloggen behouden
        int? tenantId = int.TryParse(stored.GetClaim(KampConnectClaimTypes.TenantId), out var t) ? t : null;

        var result = await claimsService.BuildIdentityAsync(userId, tenantId, ct);
        if (!result.Succeeded)
        {
            // Bv. account gedeactiveerd of rol in die organisatie kwijt → geen nieuwe tokens
            return ForbidWithError(Errors.InvalidGrant, Describe(result.Failure));
        }

        var principal = new ClaimsPrincipal(result.Identity!);
        principal.SetScopes(stored.GetScopes());       // zelfde scopes als oorspronkelijk toegekend
        principal.SetResources(new[] { options.Value.ApiAudience });
        ClaimDestinations.Apply(principal);

        // OpenIddict maakt access token (JWT, 15 min), id token en een NIEUW refresh token
        // (het oude wordt ongeldig: "rolling refresh tokens").
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    #endregion

    #region /oauth2/end-session

    /// <summary>
    /// Beëindigt de Identity-browsersessie en laat OpenIddict de OIDC-logoutrespons afhandelen.
    /// OpenIddict valideert zelf de client en de eventuele post-logout-redirect-URI.
    /// </summary>
    [HttpGet("~/oauth2/end-session")]
    public async Task<IActionResult> EndSession()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    #endregion

    #region /oauth2/userinfo

    /// <summary>
    /// Gegevens van de ingelogde gebruiker, op basis van het access token (Bearer).
    /// Welke velden terugkomen hangt af van de scopes in het token.
    /// </summary>
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/oauth2/userinfo")]
    [HttpPost("~/oauth2/userinfo")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public IActionResult UserInfo()
    {
        // Dictionary → JSON-object. Bool/int als echte JSON-types (spec: email_verified boolean, tenant_id integer).
        var info = new Dictionary<string, object>
        {
            [KampConnectClaimTypes.Subject] = User.GetClaim(Claims.Subject)!
        };

        if (User.HasScope(Scopes.Email))
        {
            if (User.GetClaim(KampConnectClaimTypes.Email) is { } email) info[KampConnectClaimTypes.Email] = email;
            info[KampConnectClaimTypes.EmailVerified] = User.GetClaim(KampConnectClaimTypes.EmailVerified) == "true";
        }

        if (int.TryParse(User.GetClaim(KampConnectClaimTypes.TenantId), out var tenantId))
        {
            info[KampConnectClaimTypes.TenantId] = tenantId;
        }

        info[KampConnectClaimTypes.Roles] = User.GetClaims(KampConnectClaimTypes.Roles).ToArray();

        if (User.GetClaim(KampConnectClaimTypes.IsPlatformAdmin) == "true")
        {
            info[KampConnectClaimTypes.IsPlatformAdmin] = true;
        }

        return Ok(info);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Weigert het request met een OAuth-fout. OpenIddict zet dit om naar het juiste formaat:
    /// JSON bij /token, redirect met ?error=… bij /authorize.
    /// </summary>
    private IActionResult ForbidWithError(string error, string description) =>
        Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            }));

    private static string Describe(UserIdentityFailure failure) => failure switch
    {
        UserIdentityFailure.UserUnavailable => "Het account bestaat niet meer of is gedeactiveerd.",
        UserIdentityFailure.TenantNotAllowed => "Je hebt geen toegang tot deze organisatie.",
        _ => "Inloggen is niet gelukt."
    };

    #endregion
}
