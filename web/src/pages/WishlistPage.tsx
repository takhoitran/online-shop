import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useCart } from '../auth/CartContext';
import { useWishlist } from '../auth/WishlistContext';
import { extractErrorMessage } from '../api/client';
import { formatCurrency } from '../utils/format';
import { Breadcrumbs } from '../components/Breadcrumbs';

export function WishlistPage() {
  const { wishlist, loading, toggle } = useWishlist();
  const { addItem } = useCart();
  const [error, setError] = useState<string | null>(null);
  const [busyProductId, setBusyProductId] = useState<string | null>(null);

  async function handleRemove(productId: string) {
    setError(null);
    setBusyProductId(productId);
    try {
      await toggle(productId);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setBusyProductId(null);
    }
  }

  async function handleAddToCart(productId: string) {
    setError(null);
    setBusyProductId(productId);
    try {
      await addItem(productId, 1);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setBusyProductId(null);
    }
  }

  if (loading && !wishlist) return <div className="loading-state">Loading wishlist…</div>;

  if (!wishlist || wishlist.items.length === 0) {
    return (
      <div>
        <Breadcrumbs items={[{ label: 'Home', to: '/' }, { label: 'Wishlist' }]} />
        <div className="empty-state">
          <p>Your wishlist is empty.</p>
          <Link to="/" className="btn">
            Continue shopping
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div>
      <Breadcrumbs items={[{ label: 'Home', to: '/' }, { label: 'Wishlist' }]} />
      <h1>Wishlist</h1>
      {error && <div className="alert error">{error}</div>}
      <div>
        {wishlist.items.map((item) => {
          const outOfStock = item.stockQuantity === 0;
          const busy = busyProductId === item.productId;
          return (
            <div className="cart-line" key={item.productId}>
              <div className="cart-line-thumb">
                {item.imageUrl ? <img src={item.imageUrl} alt={item.productName} /> : 'Image'}
              </div>
              <div>
                <Link to={`/products/${item.productId}`}>
                  <strong>{item.productName}</strong>
                </Link>
                <div className={`product-stock ${outOfStock ? 'out' : ''}`}>
                  {formatCurrency(item.price, item.currency)} {outOfStock ? '· Out of stock' : ''}
                </div>
              </div>
              <div style={{ textAlign: 'right', display: 'flex', gap: 8, alignItems: 'center', marginLeft: 'auto' }}>
                <button className="btn small" disabled={outOfStock || busy} onClick={() => handleAddToCart(item.productId)}>
                  Add to cart
                </button>
                <button className="btn secondary small" disabled={busy} onClick={() => handleRemove(item.productId)}>
                  Remove
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
