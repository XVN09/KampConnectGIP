import { useEffect, useState } from "react";
import { useAuth } from "react-oidc-context";
import type { Tenant } from "../api/types";
import { getMyTenants } from "../api/identityApi";

const TenantsPage = () => {
    const auth = useAuth();

    // Alle hooks staan bovenaan, vóór elke return: React herkent ze aan hun volgorde.
    const [tenants, setTenants] = useState<Tenant[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const accessToken = auth.user?.access_token;

    useEffect(() => {
        // Guard clause: zonder token valt er niets op te halen.
        if (!accessToken) return;

        const fetchTenants = async () => {
            try {
                const result = await getMyTenants(accessToken);
                console.log("Tenants fetched:", result);
                setTenants(result);
            } catch (err) {
                setError(err instanceof Error ? err.message : "Failed to fetch tenants");
            } finally {
                setLoading(false);
            }
        };

        fetchTenants();
        // Enkel accessToken: tenants hoort hier niet bij, want dit effect schrijft die zelf (anders: oneindige lus).
    }, [accessToken]);

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
                    <li key={t.tenantId}>{t.tenantName}</li>
                ))}
            </ul>
        </>
    );
};

export default TenantsPage;
