import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { searchProducts } from '../api/catalog';
import { ProductCard } from './ProductCard';
import { ProductGridSkeleton } from './ProductGridSkeleton';
import type { ProductListItem } from '../types';

export function BestSellersSection() {
  const [products, setProducts] = useState<ProductListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    searchProducts({ page: 1, pageSize: 40 })
      .then((r) => {
        const sorted = [...r.items].sort((a, b) => b.soldCount - a.soldCount || b.reviewCount - a.reviewCount);
        setProducts(sorted.filter((p) => p.soldCount > 0).slice(0, 8));
      })
      .finally(() => setLoading(false));
  }, []);

  if (!loading && products.length === 0) return null;

  return (
    <section className="home-section" aria-labelledby="best-sellers-heading">
      <div className="section-header">
        <div>
          <span className="section-kicker">Trending now</span>
          <h2 id="best-sellers-heading">Best sellers</h2>
        </div>
        <Link to="/products?collection=bestsellers" className="text-link">
          See all
        </Link>
      </div>
      {loading && <ProductGridSkeleton count={4} />}
      {!loading && (
        <div className="product-grid product-grid-compact">
          {products.map((p) => (
            <ProductCard key={p.id} product={p} badge="bestseller" />
          ))}
        </div>
      )}
    </section>
  );
}
