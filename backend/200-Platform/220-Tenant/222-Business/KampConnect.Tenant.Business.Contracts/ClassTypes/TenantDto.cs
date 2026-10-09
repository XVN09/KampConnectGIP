namespace KampConnect.Tenant.Business.Contracts.ClassTypes;

public record TenantDto(

    int TenantId,

    string Name,

    string Slug,

    TenantStatus Status,

    string? PrimaryEmail,

    string? PrimaryPhone,

    DateTime CreatedAt,

    DateTime? UpdatedAt

);
