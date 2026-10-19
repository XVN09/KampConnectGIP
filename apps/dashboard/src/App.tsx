import { BrowserRouter, Route, Routes } from "react-router";
import { authConfig } from "./auth/AuthConfig";
import HomePage from "./pages/HomePage";
import NotFoundPage from "./pages/NotFoundPage";
import { AuthProvider } from "react-oidc-context";
import TenantsPage from "./pages/TenantsPage";
import SignInCallback from "./pages/SignInCallback";

const App = () => {
    return (
        <AuthProvider {...authConfig}>
            <BrowserRouter>
                <Routes>
                    <Route element={<AppLayout />}>

                        <Route path="/" element={<HomePage />} />
                        <Route path="/tenants" element={<TenantsPage />} />
                        <Route path="/signin-oidc" element={<SignInCallback />} />
                        <Route path="*" element={<NotFoundPage />} />
                    </Route>
                 </Routes>
           </BrowserRouter>
        </AuthProvider>
    );
};

export default App;