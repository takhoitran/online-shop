import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import {
  createProductReview,
  deleteProductReview,
  getProductReviews,
} from '../api/catalog';
import { extractErrorMessage } from '../api/client';
import type { ProductDetail, ProductReview } from '../types';
import { formatDateTime } from '../utils/format';
import { parseProductDescription, type ProductSpecRow } from '../utils/productDescription';
import {
  adjustReviewHelpfulCount,
  getReviewHelpfulCount,
  isReviewMarkedHelpful,
  toggleReviewHelpful,
} from '../utils/reviewHelpful';
import { StarRating } from './StarRating';
import { useAuth } from '../auth/AuthContext';
import { useLanguage } from '../i18n/LanguageContext';
import { productCategoryName, productDescription, productName } from '../utils/localized';

type TabId = 'description' | 'reviews' | 'qa';

const TAB_IDS: TabId[] = ['description', 'reviews', 'qa'];

interface ProductDetailTabsProps {
  product: ProductDetail;
  onProductRefresh: () => void;
}

const REVIEWS_PAGE_SIZE = 5;

export function ProductDetailTabs({ product, onProductRefresh }: ProductDetailTabsProps) {
  const { user } = useAuth();
  const { language } = useLanguage();
  const [searchParams, setSearchParams] = useSearchParams();
  const tabParam = searchParams.get('tab') as TabId | null;
  const activeTab: TabId = TAB_IDS.includes(tabParam as TabId) ? (tabParam as TabId) : 'description';

  const displayName = productName(product, language);
  const displayDescription = productDescription(product, language);
  const displayCategoryName = productCategoryName(product, language);
  const parsed = useMemo(() => parseProductDescription(displayDescription), [displayDescription]);

  const baseSpecs: ProductSpecRow[] = useMemo(
    () => [
      { label: 'Product code', value: product.id.slice(0, 8).toUpperCase() },
      { label: 'Category', value: displayCategoryName },
      { label: 'Stock', value: product.stockQuantity > 0 ? `${product.stockQuantity} available` : 'Out of stock' },
      ...(product.soldCount > 0 ? [{ label: 'Sold', value: String(product.soldCount) }] : []),
      { label: 'Last updated', value: formatDateTime(product.updatedAtUtc) },
    ],
    [displayCategoryName, product],
  );

  const allSpecs = useMemo(() => [...parsed.specs, ...baseSpecs], [parsed.specs, baseSpecs]);

  const [reviews, setReviews] = useState<ProductReview[]>([]);
  const [reviewPage, setReviewPage] = useState(1);
  const [reviewTotalPages, setReviewTotalPages] = useState(1);
  const [reviewTotalCount, setReviewTotalCount] = useState(0);
  const [reviewsLoading, setReviewsLoading] = useState(false);
  const [ratingHistogram, setRatingHistogram] = useState<Record<number, number>>({ 1: 0, 2: 0, 3: 0, 4: 0, 5: 0 });

  const [reviewRating, setReviewRating] = useState(5);
  const [reviewComment, setReviewComment] = useState('');
  const [reviewError, setReviewError] = useState<string | null>(null);
  const [submittingReview, setSubmittingReview] = useState(false);
  const [deletingReviewId, setDeletingReviewId] = useState<string | null>(null);
  const [helpfulVersion, setHelpfulVersion] = useState(0);

  function setTab(tab: TabId) {
    const next = new URLSearchParams(searchParams);
    if (tab === 'description') next.delete('tab');
    else next.set('tab', tab);
    setSearchParams(next, { replace: true });
  }

  function loadReviews(page: number) {
    setReviewsLoading(true);
    getProductReviews(product.id, page, REVIEWS_PAGE_SIZE)
      .then((r) => {
        setReviews(r.items);
        setReviewPage(r.page);
        setReviewTotalPages(r.totalPages);
        setReviewTotalCount(r.totalCount);
      })
      .finally(() => setReviewsLoading(false));
  }

  useEffect(() => {
    loadReviews(reviewPage);
    // eslint-disable-next-line react-hooks/exhaustive-deps -- reload when product changes
  }, [product.id]);

  useEffect(() => {
    if (activeTab !== 'reviews' || reviewTotalCount === 0) return;
    getProductReviews(product.id, 1, Math.min(reviewTotalCount, 100)).then((r) => {
      const hist: Record<number, number> = { 1: 0, 2: 0, 3: 0, 4: 0, 5: 0 };
      for (const rev of r.items) {
        const star = Math.min(5, Math.max(1, rev.rating));
        hist[star] = (hist[star] ?? 0) + 1;
      }
      setRatingHistogram(hist);
    });
  }, [activeTab, product.id, reviewTotalCount]);

  async function handleSubmitReview() {
    setSubmittingReview(true);
    setReviewError(null);
    try {
      await createProductReview(product.id, reviewRating, reviewComment.trim() || null);
      setReviewComment('');
      setReviewRating(5);
      loadReviews(1);
      onProductRefresh();
    } catch (err) {
      setReviewError(extractErrorMessage(err, 'Could not submit your review.'));
    } finally {
      setSubmittingReview(false);
    }
  }

  async function handleDeleteReview(reviewId: string) {
    if (!window.confirm('Delete this review?')) return;
    setDeletingReviewId(reviewId);
    try {
      await deleteProductReview(product.id, reviewId);
      loadReviews(reviewPage);
      onProductRefresh();
    } catch (err) {
      setReviewError(extractErrorMessage(err, 'Could not delete this review.'));
    } finally {
      setDeletingReviewId(null);
    }
  }

  function handleHelpful(reviewId: string) {
    const was = isReviewMarkedHelpful(reviewId);
    const now = toggleReviewHelpful(reviewId);
    if (now && !was) adjustReviewHelpfulCount(reviewId, 1);
    if (!now && was) adjustReviewHelpfulCount(reviewId, -1);
    setHelpfulVersion((v) => v + 1);
  }

  function openAiQuestion(prefill: string) {
    window.dispatchEvent(new CustomEvent('open-ai-chat', { detail: { message: prefill } }));
  }

  const qaItems = [
    {
      q: 'Is this product in stock?',
      a: `${product.stockQuantity} unit(s) available in our warehouse.`,
    },
    {
      q: 'How long does delivery take?',
      a: 'Major cities: typically 1–3 business days after order approval; other areas: 3–5 business days (estimate).',
    },
    {
      q: 'What payment methods are accepted?',
      a: 'Cash on delivery (COD) or bank transfer — choose at checkout.',
    },
  ];

  return (
    <section className="product-detail-tabs" aria-label="Product details">
      <div className="product-detail-tablist" role="tablist">
        <button
          type="button"
          role="tab"
          id="tab-description"
          aria-selected={activeTab === 'description'}
          aria-controls="panel-description"
          className={activeTab === 'description' ? 'active' : ''}
          onClick={() => setTab('description')}
        >
          Description
        </button>
        <button
          type="button"
          role="tab"
          id="tab-reviews"
          aria-selected={activeTab === 'reviews'}
          aria-controls="panel-reviews"
          className={activeTab === 'reviews' ? 'active' : ''}
          onClick={() => setTab('reviews')}
        >
          Reviews {product.reviewCount > 0 ? `(${product.reviewCount})` : ''}
        </button>
        <button
          type="button"
          role="tab"
          id="tab-qa"
          aria-selected={activeTab === 'qa'}
          aria-controls="panel-qa"
          className={activeTab === 'qa' ? 'active' : ''}
          onClick={() => setTab('qa')}
        >
          Q&A
        </button>
      </div>

      {activeTab === 'description' && (
        <div id="panel-description" role="tabpanel" aria-labelledby="tab-description" className="product-detail-tabpanel">
          {parsed.intro && <p className="product-description-intro">{parsed.intro}</p>}
          {!parsed.intro && !displayDescription && (
            <p className="product-stock">No detailed description for this product yet.</p>
          )}
          {!parsed.intro && displayDescription && parsed.specs.length === 0 && (
            <p className="product-description-intro">{displayDescription}</p>
          )}

          <h3 className="product-specs-title">Specifications</h3>
          <table className="product-specs-table">
            <tbody>
              {allSpecs.map((row) => (
                <tr key={row.label}>
                  <th scope="row">{row.label}</th>
                  <td>{row.value}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {activeTab === 'reviews' && (
        <div id="panel-reviews" role="tabpanel" aria-labelledby="tab-reviews" className="product-detail-tabpanel">
          <div className="review-summary">
            <div className="review-summary-score">
              <strong>{product.averageRating != null ? product.averageRating.toFixed(1) : '—'}</strong>
              <StarRating rating={product.averageRating ?? 0} size={18} />
              <span className="product-stock">{reviewTotalCount} review{reviewTotalCount === 1 ? '' : 's'}</span>
            </div>
            <div className="review-summary-bars">
              {[5, 4, 3, 2, 1].map((star) => {
                const count = ratingHistogram[star] ?? 0;
                const pct = reviewTotalCount ? Math.round((count / reviewTotalCount) * 100) : 0;
                return (
                  <div key={star} className="review-bar-row">
                    <span>{star}★</span>
                    <div className="review-bar-track">
                      <div className="review-bar-fill" style={{ width: `${pct}%` }} />
                    </div>
                    <span className="product-stock">{count}</span>
                  </div>
                );
              })}
            </div>
          </div>

          {user?.role === 'Buyer' && (
            <div className="form-card review-form">
              {reviewError && <div className="alert error">{reviewError}</div>}
              <div className="field">
                <label>Your rating</label>
                <div className="review-star-picker">
                  {[1, 2, 3, 4, 5].map((star) => (
                    <button
                      key={star}
                      type="button"
                      onClick={() => setReviewRating(star)}
                      className={star <= reviewRating ? 'active' : ''}
                      aria-label={`${star} star`}
                    >
                      ★
                    </button>
                  ))}
                </div>
              </div>
              <div className="field">
                <label htmlFor="reviewComment">Comment (optional)</label>
                <textarea
                  id="reviewComment"
                  rows={3}
                  value={reviewComment}
                  onChange={(e) => setReviewComment(e.target.value)}
                  placeholder="What did you think of this product?"
                />
              </div>
              <button className="btn" disabled={submittingReview} onClick={handleSubmitReview}>
                {submittingReview ? 'Submitting…' : 'Submit review'}
              </button>
            </div>
          )}

          {reviewsLoading && <div className="loading-state">Loading reviews…</div>}
          {!reviewsLoading && reviews.length === 0 && (
            <p className="product-stock">No reviews yet — be the first to review this product.</p>
          )}

          <ul className="review-list">
            {reviews.map((r) => {
              const helpful = isReviewMarkedHelpful(r.id);
              const helpfulCount = getReviewHelpfulCount(r.id);
              void helpfulVersion;
              return (
                <li key={r.id} className="review-item">
                  <div className="review-item-header">
                    <strong>{r.buyerName}</strong>
                    <span className="product-stock">{formatDateTime(r.createdAtUtc)}</span>
                  </div>
                  <StarRating rating={r.rating} />
                  {r.comment && <p className="review-comment">{r.comment}</p>}
                  <div className="review-item-actions">
                    <button
                      type="button"
                      className={`btn secondary small ${helpful ? 'active' : ''}`}
                      onClick={() => handleHelpful(r.id)}
                    >
                      👍 Helpful {helpfulCount > 0 ? `(${helpfulCount})` : ''}
                    </button>
                    {(user?.role === 'Seller' || user?.role === 'Admin') && (
                      <button
                        className="btn danger small"
                        disabled={deletingReviewId === r.id}
                        onClick={() => handleDeleteReview(r.id)}
                      >
                        Delete
                      </button>
                    )}
                  </div>
                </li>
              );
            })}
          </ul>

          {reviewTotalPages > 1 && (
            <div className="pagination">
              <button
                type="button"
                className="btn secondary small"
                disabled={reviewPage <= 1 || reviewsLoading}
                onClick={() => loadReviews(reviewPage - 1)}
              >
                Previous
              </button>
              <span className="product-stock">
                Page {reviewPage} / {reviewTotalPages}
              </span>
              <button
                type="button"
                className="btn secondary small"
                disabled={reviewPage >= reviewTotalPages || reviewsLoading}
                onClick={() => loadReviews(reviewPage + 1)}
              >
                Next
              </button>
            </div>
          )}
        </div>
      )}

      {activeTab === 'qa' && (
        <div id="panel-qa" role="tabpanel" aria-labelledby="tab-qa" className="product-detail-tabpanel">
          <p className="product-stock" style={{ marginBottom: 16 }}>
            Common questions about this product. Need more help? Use the 🤖 AI assistant in the corner of the screen.
          </p>
          <ul className="qa-list">
            {qaItems.map((item) => (
              <li key={item.q} className="qa-item">
                <strong>{item.q}</strong>
                <p>{item.a}</p>
              </li>
            ))}
          </ul>
          <button
            type="button"
            className="btn secondary"
            onClick={() =>
              openAiQuestion(
                `Tell me about "${displayName}" (SKU ${product.id.slice(0, 8)}): stock, price, and whether it is a good fit.`,
              )
            }
          >
            🤖 Ask the assistant about this product
          </button>
        </div>
      )}
    </section>
  );
}
