import type { ProductListItem } from '../types';

/** Flash sale window resets at local midnight (demo UX). */
export function getFlashSaleEndsAt(): Date {
  const end = new Date();
  end.setHours(23, 59, 59, 999);
  return end;
}

export function getFlashSaleMsRemaining(): number {
  return Math.max(0, getFlashSaleEndsAt().getTime() - Date.now());
}

/** Pick in-stock products for today's flash row — stable for the calendar day. */
export function pickFlashSaleProducts(products: ProductListItem[], limit = 8): ProductListItem[] {
  const dayKey = new Date().toISOString().slice(0, 10);
  const inStock = products.filter((p) => p.stockQuantity > 0);
  return [...inStock]
    .sort((a, b) => scoreForFlash(a.id, dayKey) - scoreForFlash(b.id, dayKey))
    .slice(0, limit);
}

function scoreForFlash(productId: string, dayKey: string): number {
  let hash = 0;
  const key = `${dayKey}:${productId}`;
  for (let i = 0; i < key.length; i++) {
    hash = (hash * 31 + key.charCodeAt(i)) >>> 0;
  }
  return hash;
}

export function formatCountdown(ms: number): string {
  const totalSec = Math.floor(ms / 1000);
  const h = Math.floor(totalSec / 3600);
  const m = Math.floor((totalSec % 3600) / 60);
  const s = totalSec % 60;
  return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
}
