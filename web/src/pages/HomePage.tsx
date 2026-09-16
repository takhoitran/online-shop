import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getCategories, searchProducts } from '../api/catalog';
import { HeroBanner } from '../components/HeroBanner';
import { ProductCard } from '../components/ProductCard';
import { ProductGridSkeleton } from '../components/ProductGridSkeleton';
import { RecentlyViewedRow } from '../components/RecentlyViewedRow';
import { HomeCategoryStrip } from '../components/HomeCategoryStrip';
import { FlashSaleSection } from '../components/FlashSaleSection';
import { BestSellersSection } from '../components/BestSellersSection';
import { BecauseYouViewedRow } from '../components/BecauseYouViewedRow';
import type { Category, PagedResult, ProductListItem } from '../types';
import { useLanguage } from '../i18n/LanguageContext';
import { categoryDescription, categoryName } from '../utils/localized';

export function HomePage() {
  const { language } = useLanguage();
  const [categories, setCategories] = useState<Category[]>([]);
  const [newArrivals, setNewArrivals] = useState<PagedResult<ProductListItem> | null>(null);
  const [topRated, setTopRated] = useState<PagedResult<ProductListItem> | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getCategories().then(setCategories);
  }, []);

  useEffect(() => {
    setLoading(true);
    Promise.all([
      searchProducts({ page: 1, pageSize: 8 }),
      searchProducts({ page: 1, pageSize: 4, minRating: 4 }),
    ])
      .then(([latest, rated]) => {
        setNewArrivals(latest);
        setTopRated(rated.items.length > 0 ? rated : latest);
      })
      .finally(() => setLoading(false));
  }, []);

  const primaryCategory = categories[0];
  const secondaryCategory = categories[1];

  return (
    <div className="home-page">
      <HeroBanner categories={categories} />
      <HomeCategoryStrip categories={categories} />

      <section className="promo-grid" aria-label="Promotions">
        <Link to="/deals" className="promo-card promo-card-strong">
          <span>Deals hub</span>
          <strong>Vouchers &amp; flash sale</strong>
          <small>Grab promo codes and limited-time picks.</small>
        </Link>
        <Link to="/products?sort=price_asc" className="promo-card">
          <span>Deal zone</span>
          <strong>Smart buys under budget</strong>
          <small>Sort by lowest price and find quick wins.</small>
        </Link>
        <Link to="/products?minRating=4" className="promo-card">
          <span>Top rated</span>
          <strong>4-star favorites</strong>
          <small>Shop products customers already like.</small>
        </Link>
        <Link
          to={secondaryCategory ? `/categories/${secondaryCategory.id}` : primaryCategory ? `/categories/${primaryCategory.id}` : '/products'}
          className="promo-card"
        >
          <span>Featured category</span>
          <strong>
            {secondaryCategory
              ? categoryName(secondaryCategory, language)
              : primaryCategory
                ? categoryName(primaryCategory, language)
                : 'Fresh finds'}
          </strong>
          <small>Jump straight into a focused collection.</small>
        </Link>
      </section>

      <FlashSaleSection />
      <BestSellersSection />
      <BecauseYouViewedRow />

      <section className="home-section">
        <div className="section-header">
          <div>
            <span className="section-kicker">Shop by department</span>
            <h2>Popular categories</h2>
          </div>
          <Link to="/products" className="text-link">
            View all
          </Link>
        </div>
        <div className="category-tile-grid">
          {categories.map((category) => {
            const displayName = categoryName(category, language);
            return (
              <Link key={category.id} to={`/categories/${category.id}`} className="category-tile">
                <span>{displayName.slice(0, 1).toUpperCase()}</span>
                <strong>{displayName}</strong>
                <small>{categoryDescription(category, language) ?? 'Explore products selected for this category.'}</small>
              </Link>
            );
          })}
        </div>
      </section>

      <section className="home-section">
        <div className="section-header">
          <div>
            <span className="section-kicker">Just added</span>
            <h2>New arrivals</h2>
          </div>
          <Link to="/products" className="text-link">
            Shop more
          </Link>
        </div>
        {loading && <ProductGridSkeleton count={4} />}
        {!loading && newArrivals && (
          <div className="product-grid product-grid-compact">
            {newArrivals.items.slice(0, 4).map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        )}
      </section>

      {!loading && topRated && topRated.items.length > 0 && (
        <section className="home-section">
          <div className="section-header">
            <div>
              <span className="section-kicker">Customer picks</span>
              <h2>Highly rated products</h2>
            </div>
            <Link to="/products?minRating=4" className="text-link">
              See top rated
            </Link>
          </div>
          <div className="product-grid product-grid-compact">
            {topRated.items.slice(0, 4).map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        </section>
      )}

      <RecentlyViewedRow />
    </div>
  );
}
