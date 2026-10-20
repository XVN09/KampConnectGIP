
import { useEffect, useState } from "react";
import { getMyTenants } from "../api/identityApi";
import type { Tenant } from "../api/types";

export function useMyTenants(accestoken: string | undefined) {
    const [tenants, setTenants] = useState<Tenant[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!accestoken) return;
        getMyTenants(accestoken).then(result => {
            setTenants(result);
        }).catch(err => {
            setError(err instanceof Error ? err.message : "Failed to fetch tenants");
        }).finally(() => setLoading(false));
    }, [accestoken]);

    return { tenants, loading, error };
}