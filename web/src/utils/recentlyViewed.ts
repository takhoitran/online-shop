const STORAGE_KEY = 'onlineshop_recently_viewed';
const MAX_ITEMS = 10;

/** Per-browser only (localStorage), never sent to the server — a lightweight UX nicety, not
 * something that needs to sync across devices or channels like Cart/Wishlist do. */
export function addRecentlyViewed(productId: string) {
  try {
    const ids = getRecentlyViewedIds().filter((id) => id !== productId);
    ids.unshift(productId);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(ids.slice(0, MAX_ITEMS)));
  } catch {
    // localStorage unavailable (private browsing, etc.) — silently skip, it's a nicety only.
  }
}

export function getRecentlyViewedIds(excludeId?: string): string[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    const ids: string[] = raw ? JSON.parse(raw) : [];
    return excludeId ? ids.filter((id) => id !== excludeId) : ids;
  } catch {
    return [];
  }
}
