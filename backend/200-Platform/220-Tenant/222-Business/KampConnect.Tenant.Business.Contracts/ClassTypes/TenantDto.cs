using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Tenant.Business.Contracts.ClassTypes;

public enum TenantError
{
    None,
    /// <summary>Invoer ongeldig (zie <see cref="TenantResult.ValidationErrors"/>) → 400.</summary>
    Validation,
    /// <summary>Slug al in gebruik → 409.</summary>
    SlugInUse,
    /// <summary>Tenant niet gevonden → 404.</summary>
    NotFound
}

public record TenantResult(
    bool Succeeded,
    TenantError Error = TenantError.None,
    int? TenantId = null,
    IDictionary<string, string[]>? ValidationErrors = null)
{
    public static TenantResult Ok(int? tenantId = null) => new(true, TenantId: tenantId);
    public static TenantResult Fail(TenantError error) => new(false, error);
    public static TenantResult Invalid(IDictionary<string, string[]> errors) => new(false, TenantError.Validation, ValidationErrors: errors);
}
