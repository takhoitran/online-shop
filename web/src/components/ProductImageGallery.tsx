import { useCallback, useEffect, useMemo, useState } from 'react';

interface ProductImageGalleryProps {
  productName: string;
  imageUrls: string[];
  fallbackGradient: string;
  fallbackEmoji: string;
}

export function ProductImageGallery({ productName, imageUrls, fallbackGradient, fallbackEmoji }: ProductImageGalleryProps) {
  const uniqueUrls = useMemo(() => [...new Set(imageUrls.filter(Boolean))], [imageUrls]);
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [lightboxOpen, setLightboxOpen] = useState(false);

  const selectedUrl = uniqueUrls[selectedIndex] ?? null;

  useEffect(() => {
    setSelectedIndex(0);
  }, [uniqueUrls.join('|')]);

  const showPrev = useCallback(() => {
    if (uniqueUrls.length <= 1) return;
    setSelectedIndex((i) => (i - 1 + uniqueUrls.length) % uniqueUrls.length);
  }, [uniqueUrls.length]);

  const showNext = useCallback(() => {
    if (uniqueUrls.length <= 1) return;
    setSelectedIndex((i) => (i + 1) % uniqueUrls.length);
  }, [uniqueUrls.length]);

  useEffect(() => {
    if (!lightboxOpen) return;
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') setLightboxOpen(false);
      if (e.key === 'ArrowLeft') showPrev();
      if (e.key === 'ArrowRight') showNext();
    }
    document.addEventListener('keydown', onKey);
    document.body.style.overflow = 'hidden';
    return () => {
      document.removeEventListener('keydown', onKey);
      document.body.style.overflow = '';
    };
  }, [lightboxOpen, showPrev, showNext]);

  return (
    <>
      <button
        type="button"
        className="product-detail-image product-detail-image-btn"
        style={{ background: selectedUrl ? undefined : fallbackGradient }}
        onClick={() => selectedUrl && setLightboxOpen(true)}
        aria-label={selectedUrl ? 'Zoom product image' : undefined}
        disabled={!selectedUrl}
      >
        {selectedUrl ? (
          <img src={selectedUrl} alt={productName} />
        ) : (
          <span className="product-detail-image-placeholder" aria-hidden>
            {fallbackEmoji}
          </span>
        )}
        {selectedUrl && <span className="product-zoom-hint">🔍 Click to zoom</span>}
      </button>

      {uniqueUrls.length > 1 && (
        <div className="product-gallery-thumbs" role="list" aria-label="Product images">
          {uniqueUrls.map((url, index) => (
            <button
              key={url}
              type="button"
              role="listitem"
              className={`product-gallery-thumb ${index === selectedIndex ? 'active' : ''}`}
              onClick={() => setSelectedIndex(index)}
              aria-label={`Image ${index + 1}`}
              aria-current={index === selectedIndex ? 'true' : undefined}
            >
              <img src={url} alt="" />
            </button>
          ))}
        </div>
      )}

      {lightboxOpen && selectedUrl && (
        <div className="image-lightbox" role="dialog" aria-modal="true" aria-label={`Ảnh: ${productName}`}>
          <button type="button" className="image-lightbox-backdrop" aria-label="Close" onClick={() => setLightboxOpen(false)} />
          <div className="image-lightbox-inner">
            <button type="button" className="image-lightbox-close" onClick={() => setLightboxOpen(false)} aria-label="Close">
              ✕
            </button>
            {uniqueUrls.length > 1 && (
              <>
                <button type="button" className="image-lightbox-nav prev" onClick={showPrev} aria-label="Previous image">
                  ‹
                </button>
                <button type="button" className="image-lightbox-nav next" onClick={showNext} aria-label="Next image">
                  ›
                </button>
              </>
            )}
            <img src={selectedUrl} alt={productName} className="image-lightbox-img" />
            {uniqueUrls.length > 1 && (
              <div className="image-lightbox-counter">
                {selectedIndex + 1} / {uniqueUrls.length}
              </div>
            )}
          </div>
        </div>
      )}
    </>
  );
}
