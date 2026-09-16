import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { getProductById, searchProducts } from '../api/catalog';
import { useAuth } from '../auth/AuthContext';
import { useCart } from '../auth/CartContext';
import { useWishlist } from '../auth/WishlistContext';
import { extractErrorMessage } from '../api/client';
import type { ProductDetail, ProductListItem } from '../types';
import { formatCurrency } from '../utils/format';
import { getCategoryStyle } from '../utils/categoryStyle';
import { StarRating } from '../components/StarRating';
import { ProductCard } from '../components/ProductCard';
import { RecentlyViewedRow } from '../components/RecentlyViewedRow';
import { Breadcrumbs } from '../components/Breadcrumbs';
import { ProductImageGallery } from '../components/ProductImageGallery';
import { ProductDetailTabs } from '../components/ProductDetailTabs';
import { addRecentlyViewed } from '../utils/recentlyViewed';
import { useToast } from '../components/ToastProvider';
import { useLanguage } from '../i18n/LanguageContext';
import { productCategoryName, productName } from '../utils/localized';

export function ProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const { cart, addItem, updateItemQuantity } = useCart();
  const { isWishlisted, toggle } = useWishlist();
  const navigate = useNavigate();
  const { showToast } = useToast();
  const { language, t } = useLanguage();
  const [togglingWishlist, setTogglingWishlist] = useState(false);

  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [notFound, setNotFound] = useState(false);
  const [quantity, setQuantity] = useState(1);
  const [message, setMessage] = useState<{ type: 'error' | 'info'; text: string } | null>(null);
  const [adding, setAdding] = useState(false);
  const [buyingNow, setBuyingNow] = useState(false);
  const [relatedProducts, setRelatedProducts] = useState<ProductListItem[]>([]);

  const refreshProduct = useCallback(() => {
    if (!id) return;
    getProductById(id).then(setProduct).catch(() => setNotFound(true));
  }, [id]);

  useEffect(() => {
    if (!id) return;
    getProductById(id)
      .then((p) => {
        setProduct(p);
        addRecentlyViewed(p.id);
        searchProducts({ categoryId: p.categoryId, pageSize: 5 }).then((result) => {
          setRelatedProducts(result.items.filter((item) => item.id !== p.id).slice(0, 4));
        });
      })
      .catch(() => setNotFound(true));
  }, [id]);

  if (notFound) return <div className="empty-state">This product could not be found.</div>;
  if (!product) return <div className="loading-state">Loading…</div>;

  const galleryUrls = [product.imageUrl, ...product.images.map((i) => i.url)].filter((url): url is string => Boolean(url));

  async function ensureCartLineQuantity() {
    const existing = cart?.items.find((i) => i.productId === product!.id);
    if (existing) {
      await updateItemQuantity(product!.id, quantity);
    } else {
      await addItem(product!.id, quantity);
    }
  }

  async function handleBuyNow() {
    if (!user) {
      navigate('/login', { state: { from: `/products/${id}` } });
      return;
    }
    if (user.role !== 'Buyer') {
      setMessage({ type: 'info', text: 'Only Buyer accounts can purchase items.' });
      return;
    }
    setBuyingNow(true);
    setMessage(null);
    try {
      await ensureCartLineQuantity();
      navigate('/checkout', { state: { selectedProductIds: [product!.id] } });
    } catch (err) {
      setMessage({ type: 'error', text: extractErrorMessage(err) });
    } finally {
      setBuyingNow(false);
    }
  }

  async function handleAddToCart() {
    if (!user) {
      navigate('/login', { state: { from: `/products/${id}` } });
      return;
    }
    if (user.role !== 'Buyer') {
      setMessage({ type: 'info', text: 'Only Buyer accounts can add items to the cart.' });
      return;
    }
    setAdding(true);
    setMessage(null);
    try {
      await addItem(product!.id, quantity);
      showToast(t.addedToCart, 'success');
    } catch (err) {
      setMessage({ type: 'error', text: extractErrorMessage(err) });
    } finally {
      setAdding(false);
    }
  }

  async function handleToggleWishlist() {
    if (!user) {
      navigate('/login', { state: { from: `/products/${id}` } });
      return;
    }
    if (user.role !== 'Buyer' || !id) return;
    setTogglingWishlist(true);
    try {
      const wasWishlisted = id ? isWishlisted(id) : false;
      await toggle(id);
      showToast(wasWishlisted ? t.wishlistRemove : t.wishlistAdd, 'info');
    } finally {
      setTogglingWishlist(false);
    }
  }

  const outOfStock = product.stockQuantity === 0;
  const maxQuantity = Math.max(1, product.stockQuantity);
  const style = getCategoryStyle(product.categoryName);
  const wishlisted = id ? isWishlisted(id) : false;
  const displayName = productName(product, language);
  const displayCategoryName = productCategoryName(product, language);

  function clampQuantity(value: number) {
    if (outOfStock) return 1;
    return Math.min(Math.max(1, value), maxQuantity);
  }

  return (
    <>
      <Breadcrumbs
        items={[
          { label: t.home, to: '/' },
          { label: displayCategoryName, to: `/categories/${product.categoryId}` },
          { label: displayName },
        ]}
      />
      <div className="product-detail">
        <div className="product-detail-gallery-col">
          <ProductImageGallery
            productName={displayName}
            imageUrls={galleryUrls}
            fallbackGradient={style.gradient}
            fallbackEmoji={style.emoji}
          />
        </div>
        <div className="product-detail-info">
          <Link to={`/categories/${product.categoryId}`} className="user-chip">
            {displayCategoryName}
          </Link>
          <h1>{displayName}</h1>
          {product.reviewCount > 0 ? (
            <div className="product-detail-rating-row">
              <StarRating rating={product.averageRating ?? 0} />
              <span className="product-stock">
                {product.averageRating!.toFixed(1)} ({product.reviewCount} review{product.reviewCount === 1 ? '' : 's'})
              </span>
              <Link to="?tab=reviews" className="text-link">
                See reviews
              </Link>
            </div>
          ) : (
            <p className="product-stock" style={{ marginBottom: 4 }}>
              No reviews yet
            </p>
          )}
          <div className="product-detail-price">{formatCurrency(product.price, product.currency)}</div>
          <p className={`product-stock ${outOfStock ? 'out' : ''}`} style={{ marginBottom: 16 }}>
            {outOfStock ? 'Out of stock' : `${product.stockQuantity} in stock`}
            {product.soldCount > 0 && ` · ${product.soldCount} sold`}
          </p>

          {message && <div className={`alert ${message.type}`}>{message.text}</div>}

          <div className="product-detail-actions">
            <div className="qty-stepper">
              <button type="button" onClick={() => setQuantity((q) => clampQuantity(q - 1))} disabled={outOfStock}>
                −
              </button>
              <input
                value={quantity}
                onChange={(e) => setQuantity(clampQuantity(Number(e.target.value) || 1))}
                onBlur={() => setQuantity((q) => clampQuantity(q))}
                inputMode="numeric"
                max={maxQuantity}
                disabled={outOfStock}
              />
              <button
                type="button"
                onClick={() => setQuantity((q) => clampQuantity(q + 1))}
                disabled={outOfStock || quantity >= maxQuantity}
              >
                +
              </button>
            </div>
            <button className="btn" disabled={outOfStock || adding || buyingNow} onClick={handleAddToCart}>
              {adding ? 'Adding...' : t.addToCart}
            </button>
            <button className="btn accent" disabled={outOfStock || adding || buyingNow} onClick={handleBuyNow}>
              {buyingNow ? 'Redirecting...' : t.buyNow}
            </button>
            <button
              className="btn secondary"
              disabled={togglingWishlist}
              onClick={handleToggleWishlist}
              aria-label={wishlisted ? 'Remove from wishlist' : 'Add to wishlist'}
              style={wishlisted ? { color: '#e0245e', borderColor: '#e0245e' } : undefined}
            >
              {wishlisted ? '♥ Wishlisted' : '♡ Wishlist'}
            </button>
          </div>
        </div>
      </div>

      <ProductDetailTabs product={product} onProductRefresh={refreshProduct} />

      {relatedProducts.length > 0 && (
        <div className="home-section" style={{ marginTop: 40 }}>
          <h2>Related products</h2>
          <div className="product-grid">
            {relatedProducts.map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
        </div>
      )}

      <RecentlyViewedRow excludeId={id} />
    </>
  );
}
