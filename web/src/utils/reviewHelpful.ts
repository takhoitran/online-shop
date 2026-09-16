const STORAGE_KEY = 'onlineshop_review_helpful';

function readSet(): Set<string> {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    const parsed = raw ? (JSON.parse(raw) as unknown) : [];
    return new Set(Array.isArray(parsed) ? parsed.filter((x): x is string => typeof x === 'string') : []);
  } catch {
    return new Set();
  }
}

function writeSet(ids: Set<string>) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify([...ids]));
}

export function isReviewMarkedHelpful(reviewId: string): boolean {
  return readSet().has(reviewId);
}

export function toggleReviewHelpful(reviewId: string): boolean {
  const set = readSet();
  if (set.has(reviewId)) {
    set.delete(reviewId);
    writeSet(set);
    return false;
  }
  set.add(reviewId);
  writeSet(set);
  return true;
}

/** Demo counts — persisted per review in localStorage (not shared across users). */
const COUNT_KEY = 'onlineshop_review_helpful_counts';

function readCounts(): Record<string, number> {
  try {
    const raw = localStorage.getItem(COUNT_KEY);
    const parsed = raw ? (JSON.parse(raw) as unknown) : {};
    return typeof parsed === 'object' && parsed !== null ? (parsed as Record<string, number>) : {};
  } catch {
    return {};
  }
}

function writeCounts(counts: Record<string, number>) {
  localStorage.setItem(COUNT_KEY, JSON.stringify(counts));
}

export function getReviewHelpfulCount(reviewId: string): number {
  return readCounts()[reviewId] ?? 0;
}

export function adjustReviewHelpfulCount(reviewId: string, delta: number) {
  const counts = readCounts();
  const next = Math.max(0, (counts[reviewId] ?? 0) + delta);
  counts[reviewId] = next;
  writeCounts(counts);
}
