import { useAuth } from "react-oidc-context";
import { useMyTenants } from "../hooks/useMyTenants";
import { Building2 } from "lucide-react";
import { useState } from "react";

const TenantSwitcher = () => {
    const auth = useAuth();
    const accessToken = auth.user?.access_token;
    const { tenants, loading, error } = useMyTenants(accessToken);
    const activeTenantId = auth.user?.profile?.tenant_id;
    const [isSwitching, setIsSwitching] = useState(false);

    const switchTenant = async (tenantId: string) => {
        if (tenantId == null || tenantId === String(activeTenantId) || isSwitching || tenantId == "") return;
        try {
            if (isSwitching) return;
            setIsSwitching(true);
            await auth.signinRedirect({ extraQueryParams: { tenant_id: tenantId } });
        } catch (error) {
            error = error instanceof Error ? error.message : String(error);
            console.error("Failed to switch tenant:", error); 
        } finally {
            setIsSwitching(false);
        }
    };

    return (
        <div className="tenant-switcher">
            {loading && <p>Loading tenants…</p>}
            {error && <p>Error: {error}</p>}
            {tenants.length === 0 && !loading && <p>Geen organisaties gevonden.</p>}

            <div className="tenant-select-field">
                <Building2 size={20} aria-hidden="true" />
                <select aria-label="Selecteer organisatie" value={String(activeTenantId ?? "")} onChange={e => switchTenant(e.target.value)}>
                    {tenants.map(t => (
                        <option value={t.tenantId} key={t.tenantId}>
                            {t.tenantName} 
                        </option>
                    ))}
                </select>
            </div>
        </div>
    );
};

export default TenantSwitcher;