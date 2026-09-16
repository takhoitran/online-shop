export function ProductGridSkeleton({ count = 8 }: { count?: number }) {
  return (
    <div className="product-grid" aria-hidden>
      {Array.from({ length: count }, (_, i) => (
        <div key={i} className="skeleton-product-card">
          <div className="skeleton-block skeleton-thumb" />
          <div className="skeleton-block skeleton-line short" />
          <div className="skeleton-block skeleton-line" />
          <div className="skeleton-block skeleton-line medium" />
        </div>
      ))}
    </div>
  );
}
