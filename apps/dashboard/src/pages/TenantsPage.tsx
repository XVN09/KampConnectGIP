import { useAuth } from "react-oidc-context";
import { useMyTenants } from "../hooks/useMyTenants";
import { getActiveTenantId } from "../auth/tenant";

const TenantsPage = () => {
    const auth = useAuth();

    // Alle hooks staan bovenaan, vóór elke return: React herkent ze aan hun volgorde.
    const accessToken = auth.user?.access_token;
    const activeTenantId = getActiveTenantId(auth.user);
    const { tenants, loading, error } = useMyTenants(accessToken);

    // Vroege returns: pas ná alle hooks.
    if (auth.isLoading) return <p>Laden…</p>;
    if (!accessToken) return <p>Log in om je organisaties te zien.</p>;
    if (loading) return <p>Organisaties laden…</p>;
    if (error) return <p>Fout: {error}</p>;
    if (tenants.length === 0) return <p>Je bent nog geen lid van een organisatie.</p>;

    return (
        <>
            <h1>Mijn organisaties</h1>
            <ul>
                {tenants.map(t => (
                    <li key={t.tenantId}>
                        {t.tenantId === activeTenantId 
                            ? <strong> {t.tenantName} Actief </strong> 
                            : <button onClick={() => auth.signinRedirect({extraQueryParams: {tenant_id: t.tenantId }})}>{t.tenantName}</button>}
                    </li>
                ))}
            </ul>
        </>
    );
};

export default TenantsPage;
