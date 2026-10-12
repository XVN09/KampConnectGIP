using MassTransit;

namespace KampConnect.Identity.Service.Consumers;

using TenantCreated = KampConnect.Shared.Messaging.Contracts.Tenant.TenantCreated;

public class TenantCreatedConsumer: IConsumer<TenantCreated>
{
    public async Task Consume(ConsumeContext<TenantCreated> context)
    {
        var message = context.Message;
        // Log the received message
        Console.WriteLine($"Tenant created with ID {message.TenantId} by User {message.CreatedByUserId}");
    }
}
