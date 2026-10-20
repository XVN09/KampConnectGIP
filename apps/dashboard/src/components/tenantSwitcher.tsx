import { useAuth } from "react-oidc-context";
import { useMyTenants } from "../hooks/useMyTenants";
    
const TenantSwitcher = () => {
    const auth = useAuth();
    const accessToken = auth.user?.access_token;
    const { tenants, loading, error } = useMyTenants(accessToken);
    const activeTenantId = auth.user?.profile?.tenant_id;

    return (
        <div>
            {loading && <p>Loading tenants…</p>}
            {error && <p>Error: {error}</p>}
            {tenants.length === 0 && !loading && <p>Geen organisaties gevonden.</p>}
            <ul>
                {tenants.map(t => (
                    <select key={t.tenantId}>
                        <option value={t.tenantId} selected={t.tenantId === activeTenantId}>
                            {t.tenantName} {t.tenantId === activeTenantId ? "(Actief)" : ""}
                        </option>
                    </select>
                ))}
            </ul>
        </div>
    );
};

export default TenantSwitcher;