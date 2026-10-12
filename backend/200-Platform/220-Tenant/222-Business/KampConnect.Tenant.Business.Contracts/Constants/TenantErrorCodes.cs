using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Tenant.Business.Contracts.Constants;

public static class TenantErrorCodes
{
    // Slug validatie errors
    public const string SlugEmpty = "Slug.Empty";
    public const string SlugLength = "Slug.Length";
    public const string SlugFormat = "Slug.Format";
    public const string SlugReserved = "Slug.Reserved";

    // Name validatie errors
    public const string NameEmpty = "Name.Empty";
    public const string NameLength = "Name.Length";

}
