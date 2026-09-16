import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext';

/** Cart/Checkout/Orders are Buyer-only per the Use Case diagram — Seller/Admin manage the shop
 * from their own dashboard (Phase 6), not by shopping as themselves. */
export function RequireBuyer() {
  const { user } = useAuth();
  const location = useLocation();

  if (!user) {
    const returnTo = location.pathname + location.search;
    return <Navigate to="/login" state={{ from: returnTo }} replace />;
  }
  if (user.role !== 'Buyer') {
    return <Navigate to="/" replace />;
  }
  return <Outlet />;
}
