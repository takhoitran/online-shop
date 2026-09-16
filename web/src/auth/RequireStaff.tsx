import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext';

/** The whole /admin section — Seller and Admin only. */
export function RequireStaff() {
  const { user } = useAuth();
  const location = useLocation();

  if (!user) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }
  if (user.role !== 'Seller' && user.role !== 'Admin') {
    return <Navigate to="/" replace />;
  }
  return <Outlet />;
}

/** A handful of admin sub-pages (user management) are Admin-only, not Seller. */
export function RequireAdmin() {
  const { user } = useAuth();
  if (user?.role !== 'Admin') {
    return <Navigate to="/admin" replace />;
  }
  return <Outlet />;
}
