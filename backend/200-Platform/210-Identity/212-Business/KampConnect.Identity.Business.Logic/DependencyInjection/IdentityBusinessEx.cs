using KampConnect.Identity.Business.Contracts.Interfaces;
using KampConnect.Identity.Business.Logic.Implementation;
using KampConnect.Identity.Data.Entity.Users;
using Microsoft.AspNetCore.Identity;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registratie van de Business-laag van de Identity-service.</summary>
public static class IdentityBusinessEx
{
    /// <summary>
    /// Registreert de services van Business.Logic.
    /// <para>
    /// Nog nodig van de Service-laag (want ze hangen af van infrastructuur):
    /// <see cref="IIdentityMailSender"/> en <see cref="ISessionRevoker"/>,
    /// en de instellingen via Configure&lt;KampConnectIdentitySettings&gt;.
    /// </para>
    /// </summary>
    public static IServiceCollection AddIdentityBusiness(this IServiceCollection services)
    {
        // PasswordHasher van ASP.NET Identity: PBKDF2 met salt en veel iteraties.
        // Singleton: bevat geen state per gebruiker.
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IUserClaimsService, UserClaimsService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITenantMembershipService, TenantMembershipService>();
        return services;
    }
}
