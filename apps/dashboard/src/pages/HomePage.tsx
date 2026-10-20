import { useEffect } from "react";
import { useAuth } from "react-oidc-context";
import { redirect, useNavigate } from "react-router";

const HomePage = () => {
    const auth = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        if (auth.isAuthenticated) {
            if ( auth.user?.profile?.tenant_id != null) {
                navigate("/dashboard");
            } else {
                navigate("/choose-tenant");
            }
        } else {
            navigate("/signin-oidc");
        }
    }, [auth.isAuthenticated]);

    return (
        <>
            {auth.isLoading && <p>Loading...</p>}
            {auth.error && <p>Error: {auth.error.message}</p>}
            {auth.isAuthenticated ? (
                navigate("/dashboard")
            ) : (
                auth.signinRedirect()
            )}
        </>
    );
}

export default HomePage;
