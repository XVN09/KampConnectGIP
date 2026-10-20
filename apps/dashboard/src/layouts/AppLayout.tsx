import { Outlet } from 'react-router';
import TenantSwitcher from '../components/tenantSwitcher';
import './AppLayout.css';

const AppLayout = () => {
    return(
        <div className="app">
            <aside className="sidebar">
                <img src="/LogoWide.png" alt="KampConnect" />
                <TenantSwitcher />
                <nav>
                    <ul>
                        <li>Dashboard</li>
                        <li>Kampen</li>
                        <li>Chat</li>
                        <li>Instellingen</li>
                    </ul>
                </nav>
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