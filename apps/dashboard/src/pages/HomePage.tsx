import { useEffect } from "react";
import { useAuth } from "react-oidc-context";
import { redirect } from "react-router";

const HomePage = () => {
    const auth = useAuth();

    useEffect(() => {
        if (auth.isAuthenticated) {
            redirect("/dashboard");
        }
    }, [auth.isAuthenticated]);

    return (
        <>
            {auth.isLoading && <p>Loading...</p>}
            {auth.error && <p>Error: {auth.error.message}</p>}
            {!auth.isAuthenticated &&  <button onClick={() => redirect("/signin-oidc")}>Sign In</button>}
        </>
    );
}

export default HomePage;
