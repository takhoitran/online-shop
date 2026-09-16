import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function AdminLayout() {
  const { user } = useAuth();

  return (
    <div className="admin-shell">
      <aside className="admin-sidebar">
        <div className="admin-sidebar-title">Admin</div>
        <nav>
          <NavLink to="/admin/orders" className="admin-nav-link">
            📦 Orders
          </NavLink>
          <NavLink to="/admin/products" className="admin-nav-link">
            🛒 Products
          </NavLink>
          <NavLink to="/admin/categories" className="admin-nav-link">
            🏷️ Categories
          </NavLink>
          <NavLink to="/admin/inventory" className="admin-nav-link">
            📊 Inventory
          </NavLink>
          <NavLink to="/admin/ai-report" className="admin-nav-link">
            🤖 AI Report
          </NavLink>
          <NavLink to="/admin/vouchers" className="admin-nav-link">
            🎟️ Vouchers
          </NavLink>
          {user?.role === 'Admin' && (
            <NavLink to="/admin/users" className="admin-nav-link">
              👤 Users
            </NavLink>
          )}
        </nav>
      </aside>
      <div className="admin-content">
        <Outlet />
      </div>
    </div>
  );
}
