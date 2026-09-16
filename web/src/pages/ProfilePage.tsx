import { useEffect, useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { useCart } from '../auth/CartContext';
import { useWishlist } from '../auth/WishlistContext';
import { changePassword } from '../api/auth';
import { getOrders } from '../api/orders';
import { extractErrorMessage } from '../api/client';
import { Breadcrumbs } from '../components/Breadcrumbs';

export function ProfilePage() {
  const { user } = useAuth();
  const { itemCount } = useCart();
  const { wishlist } = useWishlist();
  const [orderCount, setOrderCount] = useState<number | null>(null);

  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const isBuyer = user?.role === 'Buyer';
  const isStaff = user?.role === 'Seller' || user?.role === 'Admin';

  useEffect(() => {
    if (!isBuyer) return;
    getOrders({ pageSize: 1 })
      .then((r) => setOrderCount(r.totalCount))
      .catch(() => setOrderCount(null));
  }, [isBuyer]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSuccess(false);

    if (newPassword !== confirmPassword) {
      setError('New password and confirmation do not match.');
      return;
    }

    setSubmitting(true);
    try {
      await changePassword(currentPassword, newPassword);
      setSuccess(true);
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } catch (err) {
      setError(extractErrorMessage(err, 'Could not change your password.'));
    } finally {
      setSubmitting(false);
    }
  }

  if (!user) return null;

  const initial = user.fullName.trim().charAt(0).toUpperCase() || '?';

  return (
    <div>
      <Breadcrumbs items={[{ label: 'Home', to: '/' }, { label: 'Profile' }]} />
      <h1>Profile</h1>

      <div className="form-card" style={{ margin: '0 0 20px', maxWidth: 560 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginBottom: 18 }}>
          <div className="user-avatar user-avatar-lg" style={{ width: 64, height: 64, fontSize: '1.6rem' }}>
            {initial}
          </div>
          <div>
            <div style={{ fontWeight: 700, fontSize: '1.15rem' }}>{user.fullName}</div>
            <div className="product-stock">@{user.username}</div>
            <span className="status-badge status-Approved" style={{ marginTop: 6, display: 'inline-block' }}>
              {user.role}
            </span>
          </div>
        </div>

        {isBuyer && (
          <div className="profile-stats">
            <Link to="/orders" className="profile-stat">
              <span className="profile-stat-value">{orderCount ?? '—'}</span>
              <span className="profile-stat-label">Orders</span>
            </Link>
            <Link to="/wishlist" className="profile-stat">
              <span className="profile-stat-value">{wishlist?.items.length ?? 0}</span>
              <span className="profile-stat-label">Wishlist</span>
            </Link>
            <Link to="/cart" className="profile-stat">
              <span className="profile-stat-value">{itemCount}</span>
              <span className="profile-stat-label">In cart</span>
            </Link>
          </div>
        )}
      </div>

      <div className="form-card" style={{ margin: '0 0 20px', maxWidth: 560 }}>
        <h3 style={{ marginTop: 0 }}>Account details</h3>
        <div className="profile-detail-row">
          <span className="profile-detail-label">Full name</span>
          <span>{user.fullName}</span>
        </div>
        <div className="profile-detail-row">
          <span className="profile-detail-label">Username</span>
          <span>{user.username}</span>
        </div>
        <div className="profile-detail-row">
          <span className="profile-detail-label">Role</span>
          <span>{user.role}</span>
        </div>
      </div>

      {(isBuyer || isStaff) && (
        <div className="form-card" style={{ margin: '0 0 20px', maxWidth: 560 }}>
          <h3 style={{ marginTop: 0 }}>Quick links</h3>
          <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
            {isBuyer && (
              <>
                <Link to="/orders" className="btn secondary small">
                  📦 My orders
                </Link>
                <Link to="/wishlist" className="btn secondary small">
                  ♡ Wishlist
                </Link>
                <Link to="/telegram-link" className="btn secondary small">
                  🔗 Link Telegram
                </Link>
              </>
            )}
            {isStaff && (
              <Link to="/admin" className="btn secondary small">
                🛠️ Admin panel
              </Link>
            )}
          </div>
        </div>
      )}

      <div className="form-card" style={{ maxWidth: 560 }}>
        <h3 style={{ marginTop: 0 }}>Change password</h3>
        {error && <div className="alert error">{error}</div>}
        {success && <div className="alert info">Password changed successfully.</div>}
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label htmlFor="currentPassword">Current password</label>
            <input
              id="currentPassword"
              type="password"
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="newPassword">New password</label>
            <input
              id="newPassword"
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
              minLength={8}
            />
          </div>
          <div className="field">
            <label htmlFor="confirmPassword">Confirm new password</label>
            <input
              id="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
              minLength={8}
            />
          </div>
          <button className="btn" type="submit" disabled={submitting} style={{ width: '100%' }}>
            {submitting ? 'Changing…' : 'Change password'}
          </button>
        </form>
      </div>
    </div>
  );
}
