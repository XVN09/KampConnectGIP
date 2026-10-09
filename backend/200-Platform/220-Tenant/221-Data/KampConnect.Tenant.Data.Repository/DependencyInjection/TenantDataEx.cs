using KampConnect.Tenant.Data.Interface.Repository;
using KampConnect.Tenant.Data.Repository.Implentation;

namespace Microsoft.Extensions.DependencyInjection;

public static class TenantDataEx
{
    public static IServiceCollection AddTenantData(this IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        return services;
    }
}
