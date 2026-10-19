import { useEffect } from "react";
import { useAuth } from "react-oidc-context";
import { Navigate } from "react-router";

const SignInCallback = () => {
    const auth = useAuth();

    useEffect(() => {
        if (auth.isAuthenticated) {
            window.history.replaceState(null, "", window.location.pathname);
        }
    }, [auth.isAuthenticated]);

    if (auth.isLoading) return <div>Laden...</div>;
    if (auth.error) return <div>Fout bij inloggen: {auth.error.message}</div>;
    if (auth.isAuthenticated) return <Navigate to="/" replace />;

    return <div>Bezig met inloggen...</div>;
};

export default SignInCallback;