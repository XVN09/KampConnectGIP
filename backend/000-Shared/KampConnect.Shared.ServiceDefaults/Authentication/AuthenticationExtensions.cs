using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using KampConnect.Shared.Contracts.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace KampConnect.Shared.ServiceDefaults.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddKampConnectAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationConstants.RequiresTenantPolicy, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(KampConnectClaimTypes.TenantId);
            });
            options.AddPolicy(AuthorizationConstants.RequiresPlatformAdminPolicy, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(KampConnectClaimTypes.IsPlatformAdmin, "true");
            });
            options.AddPolicy(AuthorizationConstants.RequiresAuthenticationPolicy, policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });

        return services;
    }

    public static IHostApplicationBuilder AddKampConnectJwtAuthentication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            options.Authority = builder.Configuration["Authentication:Authority"];
                            options.Audience = builder.Configuration["Authentication:Audience"];
                            options.MapInboundClaims = false;
                        });
        return builder;
    }
}
