using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Tenant.Business.Contracts.ClassTypes;

public enum TenantStatus
{
    Onboarding = 0,
    Active = 1,
    Suspended = 2,
    Closed = 3
}
