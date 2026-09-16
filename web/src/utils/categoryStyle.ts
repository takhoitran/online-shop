const CATEGORY_STYLE: Record<string, { emoji: string; gradient: string }> = {
  'T-Shirts': { emoji: '👕', gradient: 'linear-gradient(135deg, #ff8a3d, #ff4d2d)' },
  Shoes: { emoji: '👟', gradient: 'linear-gradient(135deg, #4f8fe0, #2455a4)' },
  Accessories: { emoji: '🕶️', gradient: 'linear-gradient(135deg, #d16bd1, #8a2be2)' },
  Electronics: { emoji: '🎧', gradient: 'linear-gradient(135deg, #2dd4bf, #0f766e)' },
  'Home & Kitchen': { emoji: '🏠', gradient: 'linear-gradient(135deg, #f0b866, #b45309)' },
  'Sports & Outdoors': { emoji: '🏃', gradient: 'linear-gradient(135deg, #86e078, #16803d)' },
};

const DEFAULT_STYLE = { emoji: '🛍️', gradient: 'linear-gradient(135deg, #9aa1b1, #5b6272)' };

export function getCategoryStyle(categoryName: string) {
  return CATEGORY_STYLE[categoryName] ?? DEFAULT_STYLE;
}
