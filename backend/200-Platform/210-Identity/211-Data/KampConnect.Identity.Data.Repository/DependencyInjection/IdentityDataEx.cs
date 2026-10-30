using KampConnect.Identity.Data.Interface.Repository;
using KampConnect.Identity.Data.Repository.Implementation;

// Zelfde patroon als KampConnectLoggerEx: in de namespace van DI zelf,
// zodat Program.cs geen extra using nodig heeft.
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registratie van de Data-laag van de Identity-service.</summary>
public static class IdentityDataEx
{
    /// <summary>
    /// Registreert alle repositories als <b>scoped</b> (één instantie per HTTP-request),
    /// net als de DbContext. Zo delen ze binnen één request dezelfde DbContext en dus
    /// dezelfde transactie bij SaveChangesAsync.
    /// <para>De DbContext zelf wordt in Program.cs geregistreerd (die hangt af van configuratie).</para>
    /// </summary>
    public static IServiceCollection AddIdentityData(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<ITenantSnapshotRepository, TenantSnapshotRepository>();
        services.AddScoped<ITenantInvitationRepository, TenantInvitationRepository>();

        return services;
    }
}
