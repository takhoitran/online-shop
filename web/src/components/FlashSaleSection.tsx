import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { searchProducts } from '../api/catalog';
import { ProductCard } from './ProductCard';
import { ProductGridSkeleton } from './ProductGridSkeleton';
import { formatCountdown, getFlashSaleMsRemaining, pickFlashSaleProducts } from '../utils/flashSale';
import type { ProductListItem } from '../types';

export function FlashSaleSection() {
  const [products, setProducts] = useState<ProductListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [countdown, setCountdown] = useState(() => formatCountdown(getFlashSaleMsRemaining()));

  useEffect(() => {
    searchProducts({ page: 1, pageSize: 48, sort: 'price_asc' })
      .then((r) => setProducts(pickFlashSaleProducts(r.items, 8)))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    const id = window.setInterval(() => {
      setCountdown(formatCountdown(getFlashSaleMsRemaining()));
    }, 1000);
    return () => window.clearInterval(id);
  }, []);

  if (!loading && products.length === 0) return null;

  return (
    <section className="home-section flash-sale-section" aria-labelledby="flash-sale-heading">
      <div className="section-header">
        <div>
          <span className="section-kicker flash-sale-kicker">Limited time</span>
          <h2 id="flash-sale-heading">Flash sale</h2>
        </div>
        <div className="flash-sale-meta">
          <span className="flash-sale-timer" aria-live="polite">
            Ends in <strong>{countdown}</strong>
          </span>
          <Link to="/products?collection=flash" className="text-link">
            View all
          </Link>
        </div>
      </div>
      {loading && <ProductGridSkeleton count={4} />}
      {!loading && (
        <div className="product-grid product-grid-compact">
          {products.map((p) => (
            <ProductCard key={p.id} product={p} badge="flash" />
          ))}
        </div>
      )}
    </section>
  );
}
