namespace KampConnect.Tenant.Business.Contracts.ClassTypes;

public record TenantDto(

    int tenantId,

    string name,

    string slug,

    TenantStatus status,

    string primaryEmail,

    string primaryPhone,

    DateTime CreatedAt,

    DateTime UpdatedAt

);
