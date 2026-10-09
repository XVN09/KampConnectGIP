using KampConnect.Tenant.Business.Contracts.ClassTypes;

namespace KampConnect.Tenant.Business.Logic.Helpers;

internal static class StatusHelper
{

    public static TenantStatus ParseStatus(string statusString)
    {
        if(Enum.TryParse<TenantStatus>(statusString, ignoreCase: true, out var status) && Enum.IsDefined<TenantStatus>(status))
        {
            return status;
        } else
        {
            throw new InvalidOperationException($"Invalid status string: {statusString}");
        }
    }
}
