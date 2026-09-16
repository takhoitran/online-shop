import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { getCategories, searchProducts } from '../api/catalog';
import { ProductCard } from '../components/ProductCard';
import { ProductGridSkeleton } from '../components/ProductGridSkeleton';
import { RecentlyViewedRow } from '../components/RecentlyViewedRow';
import { pickFlashSaleProducts } from '../utils/flashSale';
import type { Category, PagedResult, ProductListItem } from '../types';
import { useLanguage } from '../i18n/LanguageContext';
import { categoryName } from '../utils/localized';

const LIST_PAGE_SIZE = 12;

const SORT_OPTIONS = [
  { value: '', label: 'Newest' },
  { value: 'price_asc', label: 'Price: Low to High' },
  { value: 'price_desc', label: 'Price: High to Low' },
  { value: 'name_asc', label: 'Name: A to Z' },
];

const RATING_OPTIONS = [
  { value: '', label: 'Any rating' },
  { value: '4', label: '4 stars & up' },
  { value: '3', label: '3 stars & up' },
  { value: '2', label: '2 stars & up' },
  { value: '1', label: '1 star & up' },
];

export function ProductListingPage() {
  const { language } = useLanguage();
  const { categoryId: routeCategoryId } = useParams();
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();
  const [categories, setCategories] = useState<Category[]>([]);
  const [result, setResult] = useState<PagedResult<ProductListItem> | null>(null);
  const [loading, setLoading] = useState(true);

  const keyword = searchParams.get('keyword') ?? '';
  const queryCategoryId = searchParams.get('categoryId') ?? '';
  const categoryId = routeCategoryId ?? queryCategoryId;
  const sort = searchParams.get('sort') ?? '';
  const minPrice = searchParams.get('minPrice') ?? '';
  const maxPrice = searchParams.get('maxPrice') ?? '';
  const minRating = searchParams.get('minRating') ?? '';
  const page = Number(searchParams.get('page') ?? '1');
  const collection = searchParams.get('collection') ?? '';

  const activeCategory = useMemo(
    () => categories.find((category) => category.id === categoryId) ?? null,
    [categories, categoryId],
  );

  useEffect(() => {
    getCategories().then(setCategories);
  }, []);

  useEffect(() => {
    setLoading(true);
    const base = {
      keyword: keyword || undefined,
      categoryId: categoryId || undefined,
      minPrice: minPrice ? Number(minPrice) : undefined,
      maxPrice: maxPrice ? Number(maxPrice) : undefined,
      minRating: minRating ? Number(minRating) : undefined,
    };

    if (collection === 'bestsellers' || collection === 'flash') {
      searchProducts({
        ...base,
        sort: collection === 'flash' ? 'price_asc' : sort || undefined,
        page: 1,
        pageSize: 60,
      })
        .then((r) => {
          let items =
            collection === 'bestsellers'
              ? [...r.items].sort((a, b) => b.soldCount - a.soldCount || b.reviewCount - a.reviewCount)
              : pickFlashSaleProducts(r.items, 60);
          if (collection === 'bestsellers') {
            items = items.filter((p) => p.soldCount > 0);
          }
          const totalCount = items.length;
          const totalPages = Math.max(1, Math.ceil(totalCount / LIST_PAGE_SIZE));
          const safePage = Math.min(Math.max(1, page), totalPages);
          setResult({
            items: items.slice((safePage - 1) * LIST_PAGE_SIZE, safePage * LIST_PAGE_SIZE),
            page: safePage,
            pageSize: LIST_PAGE_SIZE,
            totalCount,
            totalPages,
          });
        })
        .finally(() => setLoading(false));
      return;
    }

    searchProducts({
      ...base,
      sort: sort || undefined,
      page,
      pageSize: LIST_PAGE_SIZE,
    })
      .then(setResult)
      .finally(() => setLoading(false));
  }, [keyword, categoryId, sort, minPrice, maxPrice, minRating, page, collection]);

  function cleanParams(params: URLSearchParams) {
    params.delete('page');
    params.delete('categoryId');
    return params.toString();
  }

  function navigateToCategory(nextCategoryId: string) {
    const next = new URLSearchParams(searchParams);
    const query = cleanParams(next);
    navigate(`${nextCategoryId ? `/categories/${nextCategoryId}` : '/products'}${query ? `?${query}` : ''}`);
  }

  function updateParam(key: string, value: string) {
    if (key === 'categoryId') {
      navigateToCategory(value);
      return;
    }

    const next = new URLSearchParams(searchParams);
    if (value) next.set(key, value);
    else next.delete(key);
    next.delete('page');
    setSearchParams(next);
  }

  function goToPage(nextPage: number) {
    const next = new URLSearchParams(searchParams);
    next.set('page', String(nextPage));
    setSearchParams(next);
  }

  const hasFilters = Boolean(keyword || categoryId || minPrice || maxPrice || minRating || collection);
  const activeCategoryName = activeCategory ? categoryName(activeCategory, language) : '';

  const listingTitle =
    collection === 'flash'
      ? 'Flash sale picks'
      : collection === 'bestsellers'
        ? 'Best sellers'
        : activeCategory
          ? `Shop ${activeCategoryName}`
          : 'Browse the Store';

  const listingKicker =
    collection === 'flash'
      ? 'Limited time'
        : collection === 'bestsellers'
          ? 'Trending'
          : activeCategory
          ? activeCategoryName
          : 'All products';

  return (
    <div>
      <div className="listing-header">
        <div>
          <span className="section-kicker">{listingKicker}</span>
          <h1>{listingTitle}</h1>
          <p>
            Compare products, filter by price and rating, then add favorites or cart items without leaving the
            collection.
          </p>
        </div>
      </div>

      <div className="category-chips">
        <button
          className={`category-chip ${categoryId === '' ? 'active' : ''}`}
          onClick={() => navigateToCategory('')}
        >
          All
        </button>
        {categories.map((c) => (
          <button
            key={c.id}
            className={`category-chip ${categoryId === c.id ? 'active' : ''}`}
            onClick={() => navigateToCategory(c.id)}
          >
            {categoryName(c, language)}
          </button>
        ))}
        <button
          type="button"
          className={`category-chip collection-chip ${collection === 'flash' ? 'active' : ''}`}
          onClick={() => updateParam('collection', collection === 'flash' ? '' : 'flash')}
        >
          ⚡ Flash sale
        </button>
        <button
          type="button"
          className={`category-chip collection-chip ${collection === 'bestsellers' ? 'active' : ''}`}
          onClick={() => updateParam('collection', collection === 'bestsellers' ? '' : 'bestsellers')}
        >
          🔥 Best sellers
        </button>
        <Link to="/deals" className="category-chip category-chip-link">
          🎟 Deals
        </Link>
      </div>

      <div className="search-bar">
        <input
          type="text"
          placeholder="Search products..."
          defaultValue={keyword}
          onKeyDown={(e) => {
            if (e.key === 'Enter') updateParam('keyword', e.currentTarget.value);
          }}
        />
        <select value={categoryId} onChange={(e) => updateParam('categoryId', e.target.value)}>
          <option value="">All categories</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>
              {categoryName(c, language)}
            </option>
          ))}
        </select>
        <input
          type="number"
          min="0"
          placeholder="Min price"
          defaultValue={minPrice}
          style={{ width: 110 }}
          onKeyDown={(e) => {
            if (e.key === 'Enter') updateParam('minPrice', e.currentTarget.value);
          }}
          onBlur={(e) => updateParam('minPrice', e.currentTarget.value)}
        />
        <input
          type="number"
          min="0"
          placeholder="Max price"
          defaultValue={maxPrice}
          style={{ width: 110 }}
          onKeyDown={(e) => {
            if (e.key === 'Enter') updateParam('maxPrice', e.currentTarget.value);
          }}
          onBlur={(e) => updateParam('maxPrice', e.currentTarget.value)}
        />
        <select value={minRating} onChange={(e) => updateParam('minRating', e.target.value)}>
          {RATING_OPTIONS.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
        <select className="sort-select" value={sort} onChange={(e) => updateParam('sort', e.target.value)}>
          {SORT_OPTIONS.map((opt) => (
            <option key={opt.value} value={opt.value}>
              Sort: {opt.label}
            </option>
          ))}
        </select>
        {(minPrice || maxPrice || minRating || keyword) && (
          <button
            className="btn secondary small"
            onClick={() => {
              const next = new URLSearchParams(searchParams);
              next.delete('keyword');
              next.delete('minPrice');
              next.delete('maxPrice');
              next.delete('minRating');
              next.delete('collection');
              next.delete('page');
              setSearchParams(next);
            }}
          >
            Clear filters
          </button>
        )}
      </div>

      {loading && <ProductGridSkeleton count={12} />}

      {!loading && result && result.items.length === 0 && (
        <div className="empty-state">No matching products found.</div>
      )}

      {!loading && result && result.items.length > 0 && (
        <>
          <div className="product-grid">
            {result.items.map((p) => (
              <ProductCard
                key={p.id}
                product={p}
                badge={collection === 'flash' ? 'flash' : collection === 'bestsellers' ? 'bestseller' : undefined}
              />
            ))}
          </div>
          {result.totalPages > 1 && (
            <div className="pagination">
              <button className="btn secondary small" disabled={page <= 1} onClick={() => goToPage(page - 1)}>
                Previous
              </button>
              <span className="user-chip">
                Page {page}/{result.totalPages}
              </span>
              <button className="btn secondary small" disabled={page >= result.totalPages} onClick={() => goToPage(page + 1)}>
                Next
              </button>
            </div>
          )}
        </>
      )}

      {!hasFilters && <RecentlyViewedRow />}
    </div>
  );
}
