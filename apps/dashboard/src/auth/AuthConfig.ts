import type { AuthProviderProps } from "react-oidc-context";

export const authConfig: AuthProviderProps = {
    authority: import.meta.env.VITE_OIDC_AUTHORITY,
    client_id: import.meta.env.VITE_OIDC_CLIENT_ID,
    redirect_uri: import.meta.env.VITE_OIDC_REDIRECT_URI,
    post_logout_redirect_uri: import.meta.env.VITE_OIDC_POST_LOGOUT_REDIRECT_URI,
    response_type: "code",
    scope: "openid email roles",
    onSigninCallback: () => {
        window.history.replaceState(null, "", window.location.pathname);
    }
};
