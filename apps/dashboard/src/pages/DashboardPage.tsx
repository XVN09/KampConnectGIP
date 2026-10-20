import { useAuth } from "react-oidc-context";
import { getActiveTenantId } from "../auth/tenant";
import { redirect, useNavigate } from "react-router";

const DashboardPage = () => {
    const auth = useAuth();
    const navigate = useNavigate();

    return (
        <>
            {auth.isLoading && <p>Loading...</p>}
            {auth.error && <p>Error: {auth.error.message}</p>}
            {auth.isAuthenticated ? (
                <p>Welcome, {auth.user?.profile.email}</p>
            ) : (
                <button onClick={() => redirect("/signin-oidc")}>Sign In</button>
            )}
            {auth.isAuthenticated && (
                <button onClick={() => auth.removeUser()}>Sign Out</button>
            )}

            <h1>Actieve Organisatie</h1> <button onClick={() => navigate("/tenants")}>Wijzig Organisatie</button>
            <p>{getActiveTenantId(auth.user) ?? "Geen actieve organisatie"}</p>
        </>
    );
}

export default DashboardPage;
