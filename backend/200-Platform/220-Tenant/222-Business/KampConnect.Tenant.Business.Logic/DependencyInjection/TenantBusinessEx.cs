using KampConnect.Tenant.Business.Logic.Implementation;
using KampConnect.Tenant.Business.Contracts.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class TenantBusinessEx
{
    public static IServiceCollection AddTenantBusiness(this IServiceCollection services)
    {
        services.AddScoped<ITenantService, TenantService>();
        return services;
    }
}

