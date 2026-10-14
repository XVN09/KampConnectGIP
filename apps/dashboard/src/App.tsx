import { BrowserRouter, Route, Routes } from "react-router";
import { authConfig } from "./auth/AuthConfig";
import HomePage from "./pages/HomePage";
import NotFoundPage from "./pages/NotFoundPage";
import { AuthProvider } from "react-oidc-context";
import TenantsPage from "./pages/TenantsPage";

const App = () => {
    return (
        <AuthProvider {...authConfig}>
            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<HomePage />} />
                    <Route path="/tenants" element={<TenantsPage />} />
                    <Route path="*" element={<NotFoundPage />} />
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    );
};

export default App;