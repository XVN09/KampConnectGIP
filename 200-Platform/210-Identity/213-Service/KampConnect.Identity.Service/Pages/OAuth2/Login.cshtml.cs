using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Constants;
using KampConnect.Identity.Business.Contracts.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace KampConnect.Identity.Service.Pages.OAuth2;

/// <summary>
/// De loginpagina (/oauth2/login).
/// <para>Zo past ze in de flow:</para>
/// <list type="number">
/// <item>De app opent /oauth2/authorize in een browser(venster).</item>
/// <item>Geen cookie → /oauth2/authorize stuurt door naar hier, met ?ReturnUrl=/oauth2/authorize?... </item>
/// <item>Gebruiker vult e-mail + wachtwoord in → <see cref="ILoginService"/> controleert.</item>
/// <item>Gelukt → cookie zetten (enkel "sub") en terug naar ReturnUrl → /oauth2/authorize gaat verder.</item>
/// </list>
/// De cookie is dus enkel het bewijs "deze browser is ingelogd"; de app krijgt nooit deze cookie,
/// maar tokens via /oauth2/token.
/// </summary>
[EnableRateLimiting(RateLimitPolicies.Auth)]
public class LoginModel(ILoginService loginService) : PageModel
{
    /// <summary>Formuliervelden.</summary>
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    /// <summary>Waar naartoe na het inloggen (komt als query-parameter binnen, gaat mee als verborgen veld).</summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    /// <summary>Foutmelding boven het formulier.</summary>
    public string? ErrorMessage { get; private set; }

    public class LoginInput
    {
        [Required(ErrorMessage = "Vul je e-mailadres in.")]
        [EmailAddress(ErrorMessage = "Geef een geldig e-mailadres.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vul je wachtwoord in.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await loginService.ValidateCredentialsAsync(Input.Email, Input.Password, ip, ct);

        if (!result.Succeeded)
        {
            // Statuscodes volgens de spec: 401 foute gegevens, 423 vergrendeld.
            (Response.StatusCode, ErrorMessage) = result.Failure switch
            {
                LoginFailure.LockedOut => (StatusCodes.Status423Locked,
                    $"Te veel mislukte pogingen. Probeer opnieuw na {result.LockedUntilUtc?.ToLocalTime():HH:mm}."),
                LoginFailure.EmailNotVerified => (StatusCodes.Status401Unauthorized,
                    "Bevestig eerst je e-mailadres via de link in je mailbox."),
                LoginFailure.Inactive => (StatusCodes.Status401Unauthorized,
                    "Dit account is gedeactiveerd."),
                _ => (StatusCodes.Status401Unauthorized,
                    "E-mailadres of wachtwoord is niet juist.")
            };
            Input.Password = string.Empty; // wachtwoord nooit terug in de pagina zetten
            return Page();
        }

        // Cookie aanmaken met enkel de user_id. Rollen/tenant worden pas bij /authorize opgebouwd.
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(KampConnectClaimTypes.Subject, result.UserId!.Value.ToString()));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        // Enkel lokale URL's toelaten: anders kan iemand een link maken die na het inloggen
        // naar een valse site stuurt ("open redirect").
        return Url.IsLocalUrl(ReturnUrl) ? LocalRedirect(ReturnUrl!) : LocalRedirect("~/");
    }
}
