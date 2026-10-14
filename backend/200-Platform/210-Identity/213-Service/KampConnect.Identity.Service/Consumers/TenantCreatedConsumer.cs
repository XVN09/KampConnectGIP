using KampConnect.Identity.Business.Contracts.Interfaces;
using MassTransit;

namespace KampConnect.Identity.Service.Consumers;

using TenantCreated = KampConnect.Shared.Messaging.Contracts.Tenant.TenantCreated;

public class TenantCreatedConsumer(ITenantMembershipService tenantMembershipService): IConsumer<TenantCreated>
{
    public async Task Consume(ConsumeContext<TenantCreated> context)
    {
        var message = context.Message;
        // Log the received message
        Console.WriteLine($"Tenant created with ID {message.TenantId} by User {message.CreatedByUserId}");
        var result = await tenantMembershipService.AddOwnerAsync(message.TenantId, message.CreatedByUserId, context.CancellationToken);
        var resultSnapshot = await tenantMembershipService.AddTenant(message.TenantId, message.TenantName, message.TenantSlug, context.CancellationToken);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to add owner for tenant {message.TenantId}: {result.Error}");
        }

        if (!resultSnapshot.Succeeded)
        {
            throw new Exception($"Failed to add tenant {message.TenantId}: {string.Join(", ", resultSnapshot.Errors.SelectMany(e => e.Value))}");
        }
    }
}
