import { NavLink, Outlet, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useState } from 'react';
import logo from '../assets/logo.png';

const getNavItems = (user) => {
  const roles = user?.roles || [];
  const isSuperAdmin = roles.includes('Super Admin');
  const isAdmin = roles.includes('Admin');
  
  const canViewProduction = isAdmin || roles.includes('Production Planner') || roles.includes('Shop Floor Supervisor') || roles.includes('Plant Manager');
  const canViewInventory = isAdmin || roles.includes('Inventory Manager') || roles.includes('Plant Manager');
  const canViewQuality = isAdmin || roles.includes('Quality Control Inspector') || roles.includes('Plant Manager');
  const canViewCosting = isAdmin || roles.includes('Cost Accountant') || roles.includes('Plant Manager');

  if (isSuperAdmin) {
    return [
      { section: 'Super Admin', items: [
        { to: '/dashboard', icon: '📊', label: 'Dashboard' },
        { to: '/companies', icon: '🏢', label: 'Companies' },
        { to: '/subscriptions', icon: '💳', label: 'Manage Subscriptions' },
        { to: '/admins', icon: '👤', label: 'Manage Admin Accounts' },
        { to: '/system-logs', icon: '📝', label: 'System Logs' },
        { to: '/backups', icon: '💾', label: 'Backups' }
      ]}
    ];
  }

  const canViewAdmin = isAdmin;

  const items = [
    { section: 'Overview', items: [
      { to: '/dashboard', icon: '📊', label: 'Dashboard' },
    ]},
  ];

  if (canViewProduction) {
    items.push({ section: 'Production', items: [
      { to: '/production', icon: '🏭', label: 'Production Planning' },
      { to: '/work-orders', icon: '📋', label: 'Work Orders' },
      { to: '/mrp', icon: '📦', label: 'MRP' },
    ]});
  }

  if (canViewInventory || canViewQuality || canViewCosting) {
    const opsItems = [];
    if (canViewInventory) opsItems.push({ to: '/inventory', icon: '🗃️', label: 'Inventory' });
    if (canViewQuality) opsItems.push({ to: '/quality', icon: '🔍', label: 'Quality Control' });
    if (canViewCosting) opsItems.push({ to: '/costing', icon: '💰', label: 'Costing' });
    items.push({ section: 'Operations', items: opsItems });
  }

  if (canViewAdmin) {
    const adminItems = [
      { to: '/users', icon: '👥', label: 'User Management' },
      { to: '/reports', icon: '📈', label: 'Reports' },
      { to: '/pricing', icon: '💎', label: 'Upgrade Plan' },
      { to: '/system-logs', icon: '📝', label: 'System Logs' }
    ];
    items.push({ section: 'Admin', items: adminItems });
  }

  return items;
};

const pageTitles = {
  '/dashboard': 'Dashboard',
  '/production': 'Production Planning',
  '/work-orders': 'Work Orders',
  '/mrp': 'Material Requirements Planning',
  '/inventory': 'Inventory Management',
  '/quality': 'Quality Control',
  '/costing': 'Production Costing',
  '/users': 'User Management',
  '/reports': 'Reports',
  '/system-logs': 'System Logs',
  '/companies': 'Companies',
  '/subscriptions': 'Manage Subscriptions',
  '/admins': 'Manage Admin Accounts',
  '/backups': 'Backups',
  '/pricing': 'Subscription Plans',
  '/payment/success': 'Payment Successful',
};

export default function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const handleLogout = () => { logout(); navigate('/login'); };
  const pageTitle = pageTitles[location.pathname] || 'OptiFlow';

  return (
    <div className="app-layout">
      <aside className={`sidebar ${sidebarOpen ? 'open' : ''}`}>
        <div className="sidebar-brand">
          <img src={logo} alt="OptiFlow Logo" style={{ width: 40, height: 40, objectFit: 'contain' }} />
          <h1>OptiFlow</h1>
        </div>
        <nav className="sidebar-nav">
          {getNavItems(user).map((sec) => (
            <div className="nav-section" key={sec.section}>
              <div className="nav-section-title">{sec.section}</div>
              {sec.items.map((item) => (
                <NavLink key={item.to} to={item.to} className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`} onClick={() => setSidebarOpen(false)}>
                  <span className="icon">{item.icon}</span>
                  {item.label}
                </NavLink>
              ))}
            </div>
          ))}
        </nav>
        <div className="sidebar-footer">
          <div className="user-card">
            <div className="user-avatar">{user?.username?.[0]?.toUpperCase() || 'U'}</div>
            <div className="user-info">
              <div className="name">{user?.fullName || user?.username || 'User'}</div>
              <div className="role">{user?.roles?.[0] || 'Role'}</div>
            </div>
          </div>
          <button className="btn btn-ghost btn-sm" style={{ width: '100%', marginTop: 8 }} onClick={handleLogout}>⏻ Logout</button>
        </div>
      </aside>
      <div className="main-area">
        <div className="topbar">
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <button className="mobile-menu-btn btn btn-ghost btn-sm" onClick={() => setSidebarOpen(!sidebarOpen)}>☰</button>
            <h2>{pageTitle}</h2>
          </div>
          <div className="topbar-actions">
            <span style={{ fontSize: 13, color: 'var(--text-muted)' }}>{user?.companyName || 'OptiFlow Manufacturing'}</span>
            <span className="badge badge-success" style={{ fontSize: 11 }}>● Online</span>
          </div>
        </div>
        <div className="main-content">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
