using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KampConnect.Shared.ServiceDefaults.Cors;

public static class CorsExtension
{
    public const string CorsPolicy = "DefaultCorsPolicy";

    public static IServiceCollection AddKampConnectCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicy, builder =>
            {
                builder.WithOrigins(corsOrigins ?? [])
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
        });
        return services;
    }
}
