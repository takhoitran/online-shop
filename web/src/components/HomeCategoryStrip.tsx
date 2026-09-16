import { Link } from 'react-router-dom';
import type { Category } from '../types';
import { getCategoryStyle } from '../utils/categoryStyle';
import { useLanguage } from '../i18n/LanguageContext';
import { categoryName } from '../utils/localized';

export function HomeCategoryStrip({ categories }: { categories: Category[] }) {
  const { language, t } = useLanguage();

  if (categories.length === 0) return null;

  return (
    <nav className="home-category-strip" aria-label="Browse by category">
      <Link to="/products" className="home-category-strip-item all">
        {t.allProducts}
      </Link>
      {categories.map((c) => {
        const style = getCategoryStyle(c.name);
        return (
          <Link key={c.id} to={`/categories/${c.id}`} className="home-category-strip-item">
            <span className="home-category-strip-emoji" aria-hidden>
              {style.emoji}
            </span>
            {categoryName(c, language)}
          </Link>
        );
      })}
    </nav>
  );
}
