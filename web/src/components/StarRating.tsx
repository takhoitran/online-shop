export function StarRating({ rating, size = 16 }: { rating: number; size?: number }) {
  const stars = [1, 2, 3, 4, 5];
  return (
    <span style={{ display: 'inline-flex', gap: 1, fontSize: size, lineHeight: 1 }}>
      {stars.map((s) => (
        <span key={s} style={{ color: s <= Math.round(rating) ? '#f5a623' : 'var(--color-border, #d0d5dd)' }}>
          ★
        </span>
      ))}
    </span>
  );
}
