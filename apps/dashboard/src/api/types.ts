export interface Tenant {
    tenantId: number;
    tenantName: string;
    tenantSlug: string | null;
}

export interface tenantCreateRequest {
    name: string;
    slug: string | null;
    primaryEmail: string;
}

export interface TenantDetails {
    tenantId: number;
    tenantName: string;
    tenantSlug: string | null;
}