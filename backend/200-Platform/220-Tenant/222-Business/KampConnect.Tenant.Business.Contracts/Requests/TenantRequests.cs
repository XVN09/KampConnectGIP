namespace KampConnect.Tenant.Business.Contracts.Requests;

// <summary>POST /api/tenant</summary>
public record TenantCreate(string Name, string Slug, string PrimaryEmail, string? PrimaryPhone);
