using System;
using System.Collections.Generic;
using System.Text;
using KampConnect.Identity.Business.Contracts.Dto;

namespace KampConnect.Identity.Business.Contracts.ClassTypes;

public record TenantsResult(bool Succeeded,
    List<TenantDto>? Tenants = null,
    IDictionary<string, string[]>? Errors = null)
{
    public static TenantsResult Success(List<TenantDto> tenants) => new(true, Tenants: tenants);

    public static TenantsResult Fail(IDictionary<string, string[]> errors) => new(false, Errors: errors);

    public static TenantsResult Error(string errorMessage) => new(false, Errors: new Dictionary<string, string[]> { { "Error", new[] { errorMessage } } });
}
