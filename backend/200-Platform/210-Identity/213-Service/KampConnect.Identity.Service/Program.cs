// =====================================================================================
//  KampConnect — Identity Service (authorization server)
// -------------------------------------------------------------------------------------
//  OAuth 2.1 + OpenID Connect via OpenIddict.
//    * Inloggen: authorization code flow met PKCE (S256)  → /oauth2/authorize + /oauth2/login
//    * Tokens:   /oauth2/token  (access token = JWT RS256, 15 min · refresh token roteert)
//    * Andere services valideren access tokens LOKAAL met de publieke sleutel uit
//      /.well-known/jwks.json — ze hoeven deze service dus niet aan te roepen per request.
//    * Accountbeheer: /api/account/... (registratie, e-mailverificatie, wachtwoordherstel)
//
//  Lagen (zie docs/KampConnect-lagen-cheatsheet.pdf):
//    Service (deze) → Business.Logic → Business.Contracts + Data.Interface
//                   → Data.Repository → Data.Context → Data.Entity
// =====================================================================================

using System.Threading.RateLimiting;
using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Interfaces;
using KampConnect.Identity.Data.Context.Implentation;
using KampConnect.Identity.Service;
using KampConnect.Identity.Service.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Abstractions.OpenIddictConstants;
using KampConnect.Shared.ServiceDefaults.Messaging;
using KampConnect.Identity.Service.Consumers;
using KampConnect.Shared.ServiceDefaults.Authentication;
using KampConnect.Shared.ServiceDefaults.Cors;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

// -------------------------------------------------------------------------------------
// 1. Instellingen (sectie "Identity" in appsettings.json)
// -------------------------------------------------------------------------------------
var identitySection = builder.Configuration.GetSection(KampConnectIdentitySettings.SectionName);
builder.Services.Configure<KampConnectIdentitySettings>(identitySection);
var identitySettings = identitySection.Get<KampConnectIdentitySettings>() ?? new KampConnectIdentitySettings();

// -------------------------------------------------------------------------------------
// 2. Database
//    UseOpenIddict<int>() voegt de 4 OpenIddict-tabellen (applications, authorizations,
//    scopes, tokens) toe aan ONZE DbContext, met int als sleuteltype.
// -------------------------------------------------------------------------------------
builder.Services.AddDbContext<KampConnectIdentityDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDb"));
    options.UseOpenIddict<int>();
});

// Data Protection: sleutels waarmee o.a. de login-cookie versleuteld wordt.
// In de database bewaren, zodat cookies geldig blijven na een herstart of met meerdere instanties.
builder.Services.AddDataProtection()
    .SetApplicationName("KampConnect.Identity")
    .PersistKeysToDbContext<KampConnectIdentityDbContext>();

// -------------------------------------------------------------------------------------
// 3. Onze lagen
// -------------------------------------------------------------------------------------
builder.Services.AddIdentityData();      // Data.Repository  → repositories
builder.Services.AddIdentityBusiness();  // Business.Logic   → LoginService, UserClaimsService, AccountService

// Infrastructuur die de businesslaag via een interface gebruikt:
builder.Services.AddScoped<ISessionRevoker, OpenIddictSessionRevoker>();
// TODO: buiten Development een echte SMTP-implementatie registreren.
builder.Services.AddSingleton<IIdentityMailSender, LoggingMailSender>();
builder.AddKampConnectMessaging(typeof(TenantCreatedConsumer).Assembly); // MassTransit + RabbitMQ

// -------------------------------------------------------------------------------------
// 4. Web: controllers (API + OAuth-endpoints), Razor Pages (loginpagina), ProblemDetails
// -------------------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddProblemDetails(); // fouten als application/problem+json (spec)

// Rate limiting: max. 10 requests/minuut per IP op login en /api/account.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// -------------------------------------------------------------------------------------
// 5. Login-cookie
//    Enkel voor de browser tijdens het inloggen: "deze browser is ingelogd als gebruiker X".
//    De app zelf gebruikt nooit deze cookie, maar tokens.
// -------------------------------------------------------------------------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/oauth2/login";
        options.Cookie.Name = ".KampConnect.Auth";
        options.Cookie.HttpOnly = true;                              // niet leesbaar vanuit JavaScript
        options.Cookie.SameSite = SameSiteMode.Lax;                  // nodig voor de redirects van de login-flow
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // lokaal ook via http; in productie altijd https
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);            // korte sessie: enkel om de flow af te ronden
        options.SlidingExpiration = true;
    });

// -------------------------------------------------------------------------------------
// 6. OpenIddict
// -------------------------------------------------------------------------------------
builder.Services.AddOpenIddict()

    // Core: waar OpenIddict zijn gegevens bewaart → onze DbContext, int-sleutels.
    .AddCore(options => options.UseEntityFrameworkCore()
        .UseDbContext<KampConnectIdentityDbContext>()
        .ReplaceDefaultEntities<int>())

    // Server: de authorization server zelf.
    .AddServer(options =>
    {
        // Issuer ("iss" in de tokens). Leeg = afgeleid uit het request.
        if (!string.IsNullOrWhiteSpace(identitySettings.Issuer))
        {
            options.SetIssuer(new Uri(identitySettings.Issuer));
        }

        // Endpoints (zelfde paden als in api/identity-service.openapi.yaml).
        // Discovery (/.well-known/openid-configuration) staat standaard aan.
        options.SetAuthorizationEndpointUris("oauth2/authorize")
               .SetTokenEndpointUris("oauth2/token")
               .SetRevocationEndpointUris("oauth2/revoke")
               .SetEndSessionEndpointUris("oauth2/end-session")
               .SetUserInfoEndpointUris("oauth2/userinfo")
               .SetJsonWebKeySetEndpointUris(".well-known/jwks.json");

        // Toegelaten flows (OAuth 2.1: geen implicit, geen password grant).
        options.AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow()
               .RequireProofKeyForCodeExchange();      // PKCE verplicht voor elke client

        // Levensduur van de tokens.
        options.SetAuthorizationCodeLifetime(TimeSpan.FromSeconds(60)) // spec: code 60 s geldig
               .SetAccessTokenLifetime(TimeSpan.FromMinutes(15))
               .SetRefreshTokenLifetime(TimeSpan.FromDays(30));
        // Refresh tokens roteren standaard: bij elk gebruik een nieuw token, het oude wordt ongeldig.

        // Scopes die clients mogen vragen.
        options.RegisterScopes(Scopes.OpenId, Scopes.Email, Scopes.OfflineAccess, Scopes.Roles);

        // Access tokens als gewone (ondertekende, niet versleutelde) JWT's,
        // zodat de andere services ze zelf kunnen valideren via de JWKS.
        options.DisableAccessTokenEncryption();

        // Sleutels/certificaten:
        //  - signing:    ondertekent de tokens (publieke sleutel staat in de JWKS)
        //  - encryption: versleutelt interne tokens (codes, refresh tokens)
        if (isDevelopment)
        {
            // Automatisch aangemaakt en in je gebruikersprofiel bewaard. NIET voor productie.
            options.AddDevelopmentEncryptionCertificate()
                   .AddDevelopmentSigningCertificate();
        }
        else
        {
            // Productie: .pfx-bestanden waarvan pad + wachtwoord in de configuratie staan
            // (Identity:Certificates:... → bij voorkeur via environment variables / secrets).
            var certs = builder.Configuration.GetSection("Identity:Certificates");
            options.AddSigningCertificate(X509CertificateLoader.LoadPkcs12FromFile(
                       certs["SigningPath"]!, certs["SigningPassword"]))
                   .AddEncryptionCertificate(X509CertificateLoader.LoadPkcs12FromFile(
                       certs["EncryptionPath"]!, certs["EncryptionPassword"]));
        }

        // Koppeling met ASP.NET Core. "Passthrough" = OpenIddict valideert het request
        // en geeft het daarna door aan onze AuthorizationController.
        var aspNetCore = options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough();

        if (isDevelopment)
        {
            // Lokaal ook via http (profiel "http" in launchSettings). In productie is https verplicht.
            aspNetCore.DisableTransportSecurityRequirement();
        }
    })

    // Validation: laat deze service zelf ook access tokens valideren (nodig voor /oauth2/userinfo).
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddAuthorization();
builder.Services.AddKampConnectCors(builder.Configuration);

// =====================================================================================
//  Pipeline — de volgorde is belangrijk!
// =====================================================================================
var app = builder.Build();

if (!isDevelopment)
{
    app.UseExceptionHandler(); // onverwachte fouten → ProblemDetails, zonder stacktrace
    app.UseHsts();
}

app.UseCors(CorsExtension.CorsPolicy);
app.UseHttpsRedirection();
app.UseStaticFiles();      // wwwroot (logo van de loginpagina)
app.UseRouting();
app.UseRateLimiter();      // na UseRouting: kent dan de policy van het endpoint
app.UseAuthentication();   // wie ben je? (cookie of token)
app.UseAuthorization();    // mag je dit?

app.MapControllers();
app.MapRazorPages();

app.Run();

/// <summary>Nodig zodat integratietests (WebApplicationFactory&lt;Program&gt;) de app kunnen starten.</summary>
public partial class Program { }
