import { useEffect, useRef } from "react";
import { useAuth } from "react-oidc-context";
import { useNavigate } from "react-router";

const HomePage = () => {
    const { isLoading, activeNavigator, error, isAuthenticated, user, signinRedirect } = useAuth();
    const navigate = useNavigate();
    const tenantId = user?.profile?.tenant_id;
    const hasRedirected = useRef(false);

    useEffect(() => {
        if (isLoading || activeNavigator || error) return;

        if (isAuthenticated) {
            navigate(tenantId != null ? "/dashboard" : "/choose-tenant", { replace: true });
        } else if (!hasRedirected.current) {
            hasRedirected.current = true;
            void signinRedirect();
        }
    }, [isLoading, activeNavigator, error, isAuthenticated, tenantId, signinRedirect, navigate]);

    return (
        <>
            {(isLoading || activeNavigator) && <p>Loading...</p>}
            {error && <p>Error: {error.message}</p>}
        </>
    );
}

export default HomePage;
