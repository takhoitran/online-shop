import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getPublicVouchers } from '../api/vouchers';
import { useAuth } from '../auth/AuthContext';
import { useToast } from '../components/ToastProvider';
import { FlashSaleSection } from '../components/FlashSaleSection';
import { formatDateTime } from '../utils/format';
import type { PublicVoucher } from '../types';
import { extractErrorMessage } from '../api/client';

function describeDiscount(v: PublicVoucher): string {
  if (v.discountType === 'Percentage') return `${v.discountValue}% off your order`;
  return `${v.discountValue.toLocaleString('en-US')} off your order`;
}

export function DealsPage() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [vouchers, setVouchers] = useState<PublicVoucher[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getPublicVouchers()
      .then(setVouchers)
      .catch((err) => setError(extractErrorMessage(err, 'Could not load promotions.')))
      .finally(() => setLoading(false));
  }, []);

  function copyCode(code: string) {
    void navigator.clipboard.writeText(code).then(
      () => showToast(`Copied ${code}`, 'success'),
      () => showToast('Could not copy to clipboard', 'error'),
    );
  }

  function useAtCheckout(code: string) {
    if (!user) {
      navigate('/login', { state: { from: '/deals' } });
      return;
    }
    if (user.role !== 'Buyer') {
      showToast('Only Buyer accounts can use vouchers at checkout.', 'info');
      return;
    }
    navigate('/checkout', { state: { voucherCode: code } });
  }

  return (
    <div className="deals-page">
      <div className="listing-header">
        <div>
          <span className="section-kicker">Save more</span>
          <h1>Deals &amp; vouchers</h1>
          <p>Copy a code and apply it at checkout, or shop today&apos;s flash sale picks.</p>
        </div>
      </div>

      <FlashSaleSection />

      <section className="home-section" aria-labelledby="vouchers-heading">
        <div className="section-header">
          <div>
            <span className="section-kicker">Promo codes</span>
            <h2 id="vouchers-heading">Available vouchers</h2>
          </div>
        </div>

        {loading && <div className="loading-state">Loading vouchers…</div>}
        {error && <div className="alert error">{error}</div>}
        {!loading && !error && vouchers.length === 0 && (
          <div className="empty-state">
            <p>No public vouchers right now — check back later or browse all products.</p>
            <Link to="/products" className="btn">
              Browse products
            </Link>
          </div>
        )}

        <ul className="voucher-public-list">
          {vouchers.map((v) => (
            <li key={v.code} className="voucher-public-card">
              <div className="voucher-public-code">{v.code}</div>
              <div>
                <strong>{describeDiscount(v)}</strong>
                {v.expiresAtUtc && (
                  <p className="product-stock">Expires {formatDateTime(v.expiresAtUtc)}</p>
                )}
              </div>
              <div className="voucher-public-actions">
                <button type="button" className="btn secondary small" onClick={() => copyCode(v.code)}>
                  Copy
                </button>
                <button type="button" className="btn small" onClick={() => useAtCheckout(v.code)}>
                  Use at checkout
                </button>
              </div>
            </li>
          ))}
        </ul>
      </section>

      <section className="promo-grid" aria-label="More ways to discover">
        <Link to="/products?collection=bestsellers" className="promo-card">
          <span>Trending</span>
          <strong>Best sellers</strong>
          <small>See what other customers are buying most.</small>
        </Link>
        <Link to="/products?minRating=4" className="promo-card">
          <span>Top rated</span>
          <strong>4-star picks</strong>
          <small>Highly rated products across the store.</small>
        </Link>
        <Link to="/products?sort=price_asc" className="promo-card promo-card-strong">
          <span>Budget</span>
          <strong>Lowest prices</strong>
          <small>Sort the catalog by price and hunt for deals.</small>
        </Link>
      </section>
    </div>
  );
}
