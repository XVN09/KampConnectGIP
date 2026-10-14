import { useAuth } from "react-oidc-context";

const HomePage = () => {
    const auth = useAuth();

    return (
        <>
            {auth.isLoading && <p>Loading...</p>}
            {auth.error && <p>Error: {auth.error.message}</p>}
            {auth.isAuthenticated ? (
                <p>Welcome, {auth.user?.profile.email}</p>
            ) : (
                <button onClick={() => auth.signinRedirect()}>Sign In</button>
            )}
            {auth.isAuthenticated && (
                <button onClick={() => auth.signoutRedirect()}>Sign Out</button>
            )}
        </>
    );
}

export default HomePage;
