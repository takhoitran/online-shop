import type { Category, ProductDetail, ProductListItem } from '../types';

type Language = 'en' | 'vi';

function choose(language: Language, fallback: string, en?: string | null, vi?: string | null) {
  const localized = language === 'vi' ? vi : en;
  return localized?.trim() || fallback;
}

function chooseNullable(language: Language, fallback?: string | null, en?: string | null, vi?: string | null) {
  const localized = language === 'vi' ? vi : en;
  return localized?.trim() || fallback || null;
}

export function productName(product: ProductListItem | ProductDetail, language: Language) {
  return choose(language, product.name, product.nameEn, product.nameVi);
}

export function productDescription(product: ProductDetail, language: Language) {
  return chooseNullable(language, product.description, product.descriptionEn, product.descriptionVi);
}

export function productCategoryName(product: ProductListItem | ProductDetail, language: Language) {
  return choose(language, product.categoryName, product.categoryNameEn, product.categoryNameVi);
}

export function categoryName(category: Category, language: Language) {
  return choose(language, category.name, category.nameEn, category.nameVi);
}

export function categoryDescription(category: Category, language: Language) {
  return chooseNullable(language, category.description, category.descriptionEn, category.descriptionVi);
}
