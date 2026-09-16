import { NavLink, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { useCart } from '../auth/CartContext';
import { useLanguage } from '../i18n/LanguageContext';

/** Mobile-first tab bar (Shopee/Lazada pattern). Hidden on admin routes and wide screens. */
export function BottomNav() {
  const { user } = useAuth();
  const { itemCount } = useCart();
  const location = useLocation();
  const { t } = useLanguage();

  if (location.pathname.startsWith('/admin')) {
    return null;
  }

  const accountTo = user ? '/profile' : '/login';

  return (
    <nav className="bottom-nav" aria-label="Main navigation">
      <NavLink to="/" end className={({ isActive }) => (isActive ? 'bottom-nav-item active' : 'bottom-nav-item')}>
        <span aria-hidden>🏠</span>
        <span>{t.home}</span>
      </NavLink>
      <NavLink to="/products" className={({ isActive }) => (isActive ? 'bottom-nav-item active' : 'bottom-nav-item')}>
        <span aria-hidden>📦</span>
        <span>{t.products}</span>
      </NavLink>
      {(!user || user.role === 'Buyer') && (
        <NavLink to="/cart" className={({ isActive }) => (isActive ? 'bottom-nav-item active' : 'bottom-nav-item')}>
          <span className="bottom-nav-icon-wrap" aria-hidden>
            🛒
            {itemCount > 0 && <span className="bottom-nav-badge">{itemCount > 99 ? '99+' : itemCount}</span>}
          </span>
          <span>{t.cart}</span>
        </NavLink>
      )}
      <NavLink to={accountTo} className={({ isActive }) => (isActive ? 'bottom-nav-item active' : 'bottom-nav-item')}>
        <span aria-hidden>👤</span>
        <span>{user ? t.account : t.login}</span>
      </NavLink>
    </nav>
  );
}
