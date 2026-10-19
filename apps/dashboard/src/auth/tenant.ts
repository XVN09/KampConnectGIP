import type { User } from "oidc-client-ts";

export function getActiveTenantId(user: User | null | undefined){
    const tenantId = Number(user?.profile.tenant_id);

    if (isNaN(tenantId)) return null;

    return tenantId;
}