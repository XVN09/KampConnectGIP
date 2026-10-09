namespace KampConnect.Tenant.Business.Contracts.ClassTypes;

public enum TenantError
{
    // Geen fout -> 200
    None,

    // Invoer ongeldig -> 400
    Validation,
    // Slug al in gebruik -> 409
    SlugInUse,

    // Tenant niet gevonden -> 404
    NotFound
}

public record TenantResult(
    bool Succeeded,
    TenantError Error = TenantError.None,
    int? TenantId = null,
    TenantDto? Tenant = null,
    IDictionary<string, string[]>? ValidationErrors = null)
{
    public static TenantResult Ok(int? tenantId = null, TenantDto? tenant = null) => new(true, TenantId: tenantId, Tenant: tenant);
    public static TenantResult Fail(TenantError error) => new(false, error);
    public static TenantResult Invalid(IDictionary<string, string[]> errors) => new(false, TenantError.Validation, ValidationErrors: errors);
}
