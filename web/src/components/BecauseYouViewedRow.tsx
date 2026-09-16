import { useEffect, useState } from 'react';
import { getProductById, searchProducts } from '../api/catalog';
import { getRecentlyViewedIds } from '../utils/recentlyViewed';
import type { ProductDetail, ProductListItem } from '../types';
import { ProductCard } from './ProductCard';
import { ProductGridSkeleton } from './ProductGridSkeleton';
import { useLanguage } from '../i18n/LanguageContext';
import { productCategoryName } from '../utils/localized';

/** Recommend products from the same category as the most recently viewed item. */
export function BecauseYouViewedRow() {
  const [products, setProducts] = useState<ProductListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [recentProduct, setRecentProduct] = useState<ProductDetail | null>(null);
  const { language } = useLanguage();

  useEffect(() => {
    const ids = getRecentlyViewedIds();
    if (ids.length === 0) {
      setProducts([]);
      setLoading(false);
      return;
    }

    getProductById(ids[0])
      .then((recent) => {
        setRecentProduct(recent);
        return searchProducts({ categoryId: recent.categoryId, pageSize: 12 });
      })
      .then((result) => {
        const viewed = new Set(ids);
        setProducts(result.items.filter((p) => !viewed.has(p.id)).slice(0, 8));
      })
      .catch(() => setProducts([]))
      .finally(() => setLoading(false));
  }, []);

  if (!loading && products.length === 0) return null;
  const recentCategoryName = recentProduct ? productCategoryName(recentProduct, language) : null;

  return (
    <section className="home-section" aria-labelledby="because-you-viewed-heading">
      <div className="section-header">
        <div>
          <span className="section-kicker">Picked for you</span>
          <h2 id="because-you-viewed-heading">
            Because you viewed{recentCategoryName ? ` · ${recentCategoryName}` : ''}
          </h2>
        </div>
      </div>
      {loading && <ProductGridSkeleton count={4} />}
      {!loading && (
        <div className="product-grid product-grid-compact">
          {products.map((p) => (
            <ProductCard key={p.id} product={p} />
          ))}
        </div>
      )}
    </section>
  );
}
