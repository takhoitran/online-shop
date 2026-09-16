/** Demo shipping fee — flat rate by region (not persisted on the order). */
export function estimateShippingFee(city: string, _subtotal: number): number {
  const normalized = city.trim().toLowerCase();
  if (!normalized) return 0;

  const metro = ['hà nội', 'ha noi', 'hanoi', 'tp.hcm', 'hồ chí minh', 'ho chi minh', 'hcm', 'đà nẵng', 'da nang'];
  if (metro.some((m) => normalized.includes(m))) {
    return 25_000;
  }
  return 35_000;
}

export function estimateDeliveryDays(city: string): string {
  const normalized = city.trim().toLowerCase();
  if (!normalized) return '2–5 business days';
  const metro = ['hà nội', 'ha noi', 'hanoi', 'tp.hcm', 'hồ chí minh', 'ho chi minh', 'hcm'];
  if (metro.some((m) => normalized.includes(m))) {
    return '1–3 business days';
  }
  return '3–5 business days';
}
