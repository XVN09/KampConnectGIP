using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Shared.Contracts.Authorization;
public static class AuthorizationConstants
{
    public const string RequiresTenantPolicy = "RequiresTenant";

    public const string RequiresPlatformAdminPolicy = "RequiresPlatformAdmin";

    public const string RequiresAuthenticationPolicy = "RequiresAuthentication";
}

