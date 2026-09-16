import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import type { ProductListItem } from '../types';
import { formatCurrency } from '../utils/format';
import { useAuth } from '../auth/AuthContext';
import { useCart } from '../auth/CartContext';
import { useWishlist } from '../auth/WishlistContext';
import { getCategoryStyle } from '../utils/categoryStyle';
import { StarRating } from './StarRating';
import { useToast } from './ToastProvider';
import { useLanguage } from '../i18n/LanguageContext';
import { productCategoryName, productName } from '../utils/localized';

export function ProductCard({
  product,
  badge,
  salePercent,
}: {
  product: ProductListItem;
  badge?: 'flash' | 'bestseller';
  salePercent?: number;
}) {
  const { user } = useAuth();
  const { addItem } = useCart();
  const { isWishlisted, toggle } = useWishlist();
  const navigate = useNavigate();
  const { showToast } = useToast();
  const { language, t } = useLanguage();
  const [adding, setAdding] = useState(false);
  const [added, setAdded] = useState(false);
  const [imgError, setImgError] = useState(false);
  const [togglingWishlist, setTogglingWishlist] = useState(false);
  const wishlisted = isWishlisted(product.id);

  async function handleToggleWishlist(e: React.MouseEvent) {
    e.preventDefault();
    e.stopPropagation();
    if (!user) {
      navigate('/login', { state: { from: `/products/${product.id}` } });
      return;
    }
    if (user.role !== 'Buyer' || togglingWishlist) return;
    setTogglingWishlist(true);
    try {
      const wasWishlisted = wishlisted;
      await toggle(product.id);
      showToast(wasWishlisted ? t.wishlistRemove : t.wishlistAdd, 'info');
    } finally {
      setTogglingWishlist(false);
    }
  }

  const style = getCategoryStyle(product.categoryName);
  const displayName = productName(product, language);
  const displayCategoryName = productCategoryName(product, language);
  const showImage = Boolean(product.imageUrl) && !imgError;
  const outOfStock = product.stockQuantity === 0;
  const lowStock = !outOfStock && product.stockQuantity <= 8;
  const effectiveSalePercent = !outOfStock ? salePercent ?? (badge === 'flash' ? 18 : undefined) : undefined;
  const originalPrice =
    effectiveSalePercent && effectiveSalePercent > 0
      ? Math.round(product.price / (1 - effectiveSalePercent / 100))
      : null;

  async function handleQuickAdd(e: React.MouseEvent) {
    e.preventDefault();
    e.stopPropagation();
    if (!user) {
      navigate('/login', { state: { from: `/products/${product.id}` } });
      return;
    }
    if (user.role !== 'Buyer' || outOfStock || adding) return;

    setAdding(true);
    try {
      await addItem(product.id, 1);
      setAdded(true);
      showToast(t.addedToCart, 'success');
      setTimeout(() => setAdded(false), 1500);
    } finally {
      setAdding(false);
    }
  }

  return (
    <div className="product-card">
      {badge === 'flash' && !outOfStock && <span className="product-badge flash">Flash sale</span>}
      {badge === 'bestseller' && !outOfStock && (
        <span className="product-badge bestseller">Best seller</span>
      )}
      {outOfStock && <span className="product-badge out">Sold out</span>}
      {lowStock && !badge && <span className="product-badge">Only {product.stockQuantity} left</span>}

      {(!user || user.role === 'Buyer') && (
        <button
          className={`wishlist-btn ${wishlisted ? 'active' : ''}`}
          onClick={handleToggleWishlist}
          disabled={togglingWishlist}
          aria-label={wishlisted ? 'Remove from wishlist' : 'Add to wishlist'}
          title={wishlisted ? 'Remove from wishlist' : 'Add to wishlist'}
        >
          {wishlisted ? '♥' : '♡'}
        </button>
      )}

      {(!user || user.role === 'Buyer') && (
        <button
          className={`quick-add-btn ${added ? 'added' : ''}`}
          onClick={handleQuickAdd}
          disabled={outOfStock || adding}
          aria-label="Add to cart"
          title="Add to cart"
        >
          {added ? '✓' : '🛒'}
        </button>
      )}

      <Link to={`/products/${product.id}`} className="product-thumb" style={{ background: showImage ? undefined : style.gradient }}>
        {showImage ? (
          <img src={product.imageUrl!} alt={displayName} onError={() => setImgError(true)} />
        ) : (
          <span className="product-thumb-emoji">{style.emoji}</span>
        )}
      </Link>
      <Link to={`/products/${product.id}`} className="product-card-body">
        <span className="product-category">{displayCategoryName}</span>
        <h3>{displayName}</h3>
        {product.reviewCount > 0 && (
          <span style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
            <StarRating rating={product.averageRating ?? 0} size={13} />
            <span className="product-stock">({product.reviewCount})</span>
          </span>
        )}
        {originalPrice ? (
          <span className="product-price-row">
            <span className="product-price-old">{formatCurrency(originalPrice, product.currency)}</span>
            <span className="product-price sale">{formatCurrency(product.price, product.currency)}</span>
            <span className="sale-percent">-{effectiveSalePercent}%</span>
          </span>
        ) : (
          <span className="product-price">{formatCurrency(product.price, product.currency)}</span>
        )}
        <span className={`product-stock ${outOfStock ? 'out' : ''}`}>
          {outOfStock ? 'Out of stock' : `${product.stockQuantity} in stock`}
          {product.soldCount > 0 && ` · ${product.soldCount} sold`}
        </span>
      </Link>
    </div>
  );
}
