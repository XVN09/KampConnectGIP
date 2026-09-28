using System.Security.Claims;

namespace KampConnect.Identity.Business.Contracts.ClassTypes;

/// <summary>Waarom er geen identiteit (claims) opgebouwd kon worden.</summary>
public enum UserIdentityFailure
{
    None,
    /// <summary>Gebruiker bestaat niet (meer) of is gedeactiveerd.</summary>
    UserUnavailable,
    /// <summary>De gebruiker hoort bij meerdere organisaties en er werd geen tenant_id meegegeven.</summary>
    TenantRequired,
    /// <summary>De gevraagde tenant_id is niet een organisatie van deze gebruiker.</summary>
    TenantNotAllowed
}

/// <summary>
/// Resultaat van <c>IUserClaimsService.BuildIdentityAsync</c>.
/// <see cref="Identity"/> is een gewone .NET ClaimsIdentity, zonder iets van OpenIddict:
/// de Service-laag voegt daarna de OpenIddict-specifieke zaken toe (scopes, destinations).
/// </summary>
public record UserIdentityResult(ClaimsIdentity? Identity, UserIdentityFailure Failure)
{
    public bool Succeeded => Identity is not null;
    public static UserIdentityResult Success(ClaimsIdentity identity) => new(identity, UserIdentityFailure.None);
    public static UserIdentityResult Fail(UserIdentityFailure failure) => new(null, failure);
}
