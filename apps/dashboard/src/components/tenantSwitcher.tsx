import { useAuth } from "react-oidc-context";
import { useMyTenants } from "../hooks/useMyTenants";
    
const TenantSwitcher = () => {
    const auth = useAuth();
    const accessToken = auth.user?.access_token;
    const { tenants, loading, error } = useMyTenants(accessToken);
    const activeTenantId = auth.user?.profile?.tenant_id;

    return (
        <div className="tenant-switcher">
            {loading && <p>Loading tenants…</p>}
            {error && <p>Error: {error}</p>}
            {tenants.length === 0 && !loading && <p>Geen organisaties gevonden.</p>}

            <select aria-label="Selecteer organisatie">
                {tenants.map(t => (
                    <option value={t.tenantId} selected={t.tenantId === activeTenantId} key={t.tenantId}>
                        {t.tenantName} 
                    </option>
                ))}
            </select>
        </div>
    );
};

export default TenantSwitcher;