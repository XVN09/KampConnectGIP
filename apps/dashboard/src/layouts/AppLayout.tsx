import { NavLink, Outlet } from 'react-router';
import { useAuth } from 'react-oidc-context';
import TenantSwitcher from '../components/tenantSwitcher';
import './AppLayout.css';
import { LayoutDashboard, LogOut, MessageCircle, Settings } from 'lucide-react';

const AppLayout = () => {
    const auth = useAuth();

    return(
        <div className="app">
            <aside className="sidebar">
                <img src="/LogoWide.png" alt="KampConnect" />
                <TenantSwitcher />
                <nav>
                    <ul>
                        <li><NavLink to="/dashboard"><LayoutDashboard size={16} aria-hidden="true" />Dashboard</NavLink></li>
                        <li><NavLink to="/chat"><MessageCircle size={16} aria-hidden="true" />Chat</NavLink></li>
                        <li><NavLink to="/instellingen"><Settings size={16} aria-hidden="true" />Instellingen</NavLink></li>
                    </ul>
                </nav>
                <button className="signout-button" onClick={() => auth.signoutRedirect()}><LogOut size={16} aria-hidden="true" />Uitloggen</button>
            </aside>
            <div className="main-area">
                <header className="topbar">
                    <h1>Topbar</h1>
                </header>
                <main className="content">
                    <Outlet />
                </main>
            </div>
        </div>
    )
};

export default AppLayout;