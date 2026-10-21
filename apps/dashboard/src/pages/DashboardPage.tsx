import { useAuth } from "react-oidc-context";
import { getActiveTenantId } from "../auth/tenant";
import { useNavigate } from "react-router";
import { useEffect } from "react";

const DashboardPage = () => {
    const auth = useAuth();
    const { isLoading, activeNavigator, error, isAuthenticated } = auth;
    const navigate = useNavigate();
    useEffect(() => {
        if (isLoading || activeNavigator || error) return;

        if (!isAuthenticated) {
            navigate("/", { replace: true });
        }
    }, [isLoading, activeNavigator, error, isAuthenticated, navigate]);
    return (
        <>
            {auth.isLoading && <p>Loading...</p>}
            {auth.error && <p>Error: {auth.error.message}</p>}
            {auth.isAuthenticated && <p>Welkom, {auth.user?.profile.email}</p>}
            <h1>Actieve Organisatie</h1> <button onClick={() => navigate("/tenants")}>Wijzig Organisatie</button>
            <p>{getActiveTenantId(auth.user) ?? "Geen actieve organisatie"}</p>
        </>
    );
}

export default DashboardPage;
