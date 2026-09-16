import { useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useCart } from '../auth/CartContext';
import { extractErrorMessage } from '../api/client';
import { formatCurrency } from '../utils/format';
import { Breadcrumbs } from '../components/Breadcrumbs';

export function CartPage() {
  const { cart, loading, updateItemQuantity, removeItem } = useCart();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [busyProductId, setBusyProductId] = useState<string | null>(null);

  // Only track what the user has explicitly unchecked — everything else (including any item
  // never seen before, e.g. freshly added) is selected by default. This is a pure derivation,
  // no effect needed to "sync" it as the cart changes.
  const [deselected, setDeselected] = useState<Set<string>>(new Set());
  const selected = useMemo(
    () => new Set((cart?.items ?? []).filter((i) => !deselected.has(i.productId)).map((i) => i.productId)),
    [cart, deselected],
  );

  async function handleQuantityChange(productId: string, quantity: number) {
    setError(null);
    setBusyProductId(productId);
    try {
      await updateItemQuantity(productId, quantity);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setBusyProductId(null);
    }
  }

  function toggleSelected(productId: string) {
    setDeselected((prev) => {
      const next = new Set(prev);
      if (next.has(productId)) next.delete(productId);
      else next.add(productId);
      return next;
    });
  }

  function toggleSelectAll(checked: boolean) {
    if (!cart) return;
    setDeselected(checked ? new Set() : new Set(cart.items.map((i) => i.productId)));
  }

  function handleCheckout() {
    navigate('/checkout', { state: { selectedProductIds: Array.from(selected) } });
  }

  if (loading && !cart) return <div className="loading-state">Loading cart…</div>;

  if (!cart || cart.items.length === 0) {
    return (
      <div>
        <Breadcrumbs items={[{ label: 'Home', to: '/' }, { label: 'Cart' }]} />
        <div className="empty-state">
          <p>Your cart is empty.</p>
          <Link to="/" className="btn">
            Continue shopping
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div>
      <Breadcrumbs items={[{ label: 'Home', to: '/' }, { label: 'Cart' }]} />
      <h1>Cart</h1>
      {error && <div className="alert error">{error}</div>}
      <div className="cart-layout">
        <div>
          <label className="cart-select-all">
            <input
              type="checkbox"
              checked={selected.size === cart.items.length}
              ref={(el) => {
                if (el) el.indeterminate = selected.size > 0 && selected.size < cart.items.length;
              }}
              onChange={(e) => toggleSelectAll(e.target.checked)}
            />
            Select all ({selected.size}/{cart.items.length})
          </label>

          {cart.items.map((item) => (
            <div className="cart-line" key={item.productId}>
              <input
                type="checkbox"
                className="cart-line-checkbox"
                checked={selected.has(item.productId)}
                onChange={() => toggleSelected(item.productId)}
                aria-label={`Select ${item.productName} for checkout`}
              />
              <div className="cart-line-thumb">
                {item.imageUrl ? <img src={item.imageUrl} alt={item.productName} /> : 'Image'}
              </div>
              <div>
                <Link to={`/products/${item.productId}`}>
                  <strong>{item.productName}</strong>
                </Link>
                <div className="product-stock">{formatCurrency(item.unitPrice, item.currency)} / item</div>
              </div>
              <div className="qty-stepper">
                <button
                  type="button"
                  disabled={busyProductId === item.productId}
                  onClick={() => handleQuantityChange(item.productId, item.quantity - 1)}
                >
                  −
                </button>
                <input readOnly value={item.quantity} />
                <button
                  type="button"
                  disabled={busyProductId === item.productId}
                  onClick={() => handleQuantityChange(item.productId, item.quantity + 1)}
                >
                  +
                </button>
              </div>
              <div style={{ textAlign: 'right' }}>
                <div style={{ fontWeight: 700 }}>{formatCurrency(item.lineTotal, item.currency)}</div>
                <button className="btn secondary small" style={{ marginTop: 6 }} onClick={() => removeItem(item.productId)}>
                  Remove
                </button>
              </div>
            </div>
          ))}
        </div>

        <div className="summary-card">
          <div className="summary-row total">
            <span>Total for selected items</span>
            <span>
              {formatCurrency(
                cart.items.filter((i) => selected.has(i.productId)).reduce((sum, i) => sum + i.lineTotal, 0),
                cart.currency,
              )}
            </span>
          </div>
          <button className="btn" style={{ width: '100%', marginTop: 12 }} disabled={selected.size === 0} onClick={handleCheckout}>
            Proceed to checkout {selected.size > 0 ? `(${selected.size})` : ''}
          </button>
        </div>
      </div>
    </div>
  );
}
