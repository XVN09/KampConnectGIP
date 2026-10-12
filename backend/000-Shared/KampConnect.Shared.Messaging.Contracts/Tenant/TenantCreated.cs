using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Shared.Messaging.Contracts.Tenant;

public record TenantCreated
{
    public int TenantId { get; init; }

    public int CreatedByUserId { get; init; }
}
