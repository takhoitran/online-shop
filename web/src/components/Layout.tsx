import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { useCart } from '../auth/CartContext';
import { useWishlist } from '../auth/WishlistContext';
import { AiChatWidget } from './AiChatWidget';
import { NotificationBell } from './NotificationBell';
import { UserMenu } from './UserMenu';
import { BottomNav } from './BottomNav';
import { HeaderSearch } from './HeaderSearch';
import { useLanguage } from '../i18n/LanguageContext';

export function Layout() {
  const { user } = useAuth();
  const { itemCount } = useCart();
  const { wishlist } = useWishlist();
  const { language, t, toggleLanguage } = useLanguage();

  return (
    <>
      <header className="site-header">
        <div className="container">
          <NavLink to="/" className="brand">
            🛍️ OnlineShop
          </NavLink>
          <HeaderSearch />
          <nav className="nav-links hide-on-mobile">
            <NavLink to="/">{t.home}</NavLink>
            <NavLink to="/products">{t.products}</NavLink>
            <NavLink to="/deals">{t.deals}</NavLink>
            {user?.role === 'Buyer' && (
              <>
                <NavLink to="/orders">{t.myOrders}</NavLink>
                <NavLink to="/telegram-link">Telegram</NavLink>
              </>
            )}
            {(user?.role === 'Seller' || user?.role === 'Admin') && <NavLink to="/admin">Admin panel</NavLink>}
          </nav>
          <div className="header-right">
            <button
              type="button"
              className="language-toggle"
              onClick={toggleLanguage}
              aria-label="Switch language"
              title="Switch language"
            >
              {language === 'vi' ? 'VI' : 'EN'}
            </button>
            {user ? (
              <>
                {user.role === 'Buyer' && (
                  <>
                    <NavLink to="/wishlist" className="icon-link" aria-label="Wishlist">
                      <span className="icon-link-glyph">♡</span>
                      {(wishlist?.items.length ?? 0) > 0 && <span className="cart-badge">{wishlist!.items.length}</span>}
                    </NavLink>
                    <NavLink to="/cart" className="icon-link" aria-label="Cart">
                      <span className="icon-link-glyph">🛒</span>
                      {itemCount > 0 && <span className="cart-badge">{itemCount}</span>}
                    </NavLink>
                  </>
                )}
                <NotificationBell />
                <UserMenu />
              </>
            ) : (
              <>
                <NavLink to="/login" className="btn secondary small">
                  {t.login}
                </NavLink>
                <NavLink to="/register" className="btn small">
                  {t.signup}
                </NavLink>
              </>
            )}
          </div>
        </div>
      </header>
      <main className="page">
        <div className="container">
          <Outlet />
        </div>
      </main>
      <BottomNav />
      <AiChatWidget />
    </>
  );
}
