import { useEffect, useState } from 'react';
import { getProductById } from '../api/catalog';
import { getRecentlyViewedIds } from '../utils/recentlyViewed';
import type { ProductDetail } from '../types';
import { ProductCard } from './ProductCard';

export function RecentlyViewedRow({ excludeId }: { excludeId?: string }) {
  const [products, setProducts] = useState<ProductDetail[]>([]);

  useEffect(() => {
    const ids = getRecentlyViewedIds(excludeId).slice(0, 6);
    if (ids.length === 0) {
      setProducts([]);
      return;
    }
    Promise.all(ids.map((id) => getProductById(id).catch(() => null))).then((results) => {
      setProducts(results.filter((p): p is ProductDetail => p !== null));
    });
  }, [excludeId]);

  if (products.length === 0) return null;

  return (
    <div style={{ marginTop: 40 }}>
      <h2>Recently viewed</h2>
      <div className="product-grid">
        {products.map((p) => (
          <ProductCard key={p.id} product={p} />
        ))}
      </div>
    </div>
  );
}
