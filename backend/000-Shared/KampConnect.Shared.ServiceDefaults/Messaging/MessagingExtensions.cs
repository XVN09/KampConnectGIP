using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


namespace KampConnect.Shared.ServiceDefaults.Messaging;

public static class MessagingExtensions
{
    public static IHostApplicationBuilder AddKampConnectMessaging(
        this IHostApplicationBuilder builder,
        params Assembly[] consumerAssemblies)
    {
        const string MessagingConnectionName = "messaging";
        var connectionString = builder.Configuration.GetConnectionString(MessagingConnectionName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{MessagingConnectionName}' is not configured. Configure ConnectionStrings:messaging via user-secrets in the AppHost project.");
        }

        builder.Services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            foreach (var assembly in consumerAssemblies)
            {
                x.AddConsumers(assembly);
            }

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(connectionString);
                cfg.ConfigureEndpoints(context);
            });
        });

        return builder; 
    }
}
