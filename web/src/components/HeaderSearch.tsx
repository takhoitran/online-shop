import { useEffect, useRef, useState, type FormEvent } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { searchProducts } from '../api/catalog';
import type { ProductListItem } from '../types';
import { useLanguage } from '../i18n/LanguageContext';
import { productCategoryName, productName } from '../utils/localized';

const HISTORY_KEY = 'onlineshop_search_history';
const MAX_HISTORY = 8;

function readHistory(): string[] {
  try {
    const raw = localStorage.getItem(HISTORY_KEY);
    const parsed = raw ? (JSON.parse(raw) as unknown) : [];
    return Array.isArray(parsed) ? parsed.filter((x): x is string => typeof x === 'string').slice(0, MAX_HISTORY) : [];
  } catch {
    return [];
  }
}

function writeHistory(items: string[]) {
  localStorage.setItem(HISTORY_KEY, JSON.stringify(items.slice(0, MAX_HISTORY)));
}

export function HeaderSearch() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const rootRef = useRef<HTMLDivElement>(null);
  const [query, setQuery] = useState(() => searchParams.get('keyword') ?? '');
  const [open, setOpen] = useState(false);
  const [history, setHistory] = useState<string[]>(() => readHistory());
  const [suggestions, setSuggestions] = useState<ProductListItem[]>([]);
  const [loadingSuggestions, setLoadingSuggestions] = useState(false);
  const { language, t } = useLanguage();

  useEffect(() => {
    setQuery(searchParams.get('keyword') ?? '');
  }, [searchParams]);

  useEffect(() => {
    if (!open) return;
    function onDocClick(e: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener('mousedown', onDocClick);
    return () => document.removeEventListener('mousedown', onDocClick);
  }, [open]);

  useEffect(() => {
    const trimmed = query.trim();
    if (trimmed.length < 2) {
      setSuggestions([]);
      return;
    }
    const handle = window.setTimeout(() => {
      setLoadingSuggestions(true);
      searchProducts({ keyword: trimmed, pageSize: 6 })
        .then((r) => setSuggestions(r.items))
        .catch(() => setSuggestions([]))
        .finally(() => setLoadingSuggestions(false));
    }, 280);
    return () => window.clearTimeout(handle);
  }, [query]);

  function pushHistory(term: string) {
    const next = [term, ...history.filter((h) => h !== term)].slice(0, MAX_HISTORY);
    setHistory(next);
    writeHistory(next);
  }

  function goSearch(term: string) {
    const trimmed = term.trim();
    if (!trimmed) {
      navigate('/products');
      setOpen(false);
      return;
    }
    pushHistory(trimmed);
    setOpen(false);
    navigate(`/products?keyword=${encodeURIComponent(trimmed)}`);
  }

  function onSubmit(e: FormEvent) {
    e.preventDefault();
    goSearch(query);
  }

  return (
    <div ref={rootRef} className="header-search">
      <form onSubmit={onSubmit} role="search">
        <input
          type="search"
          className="header-search-input"
          placeholder={t.searchPlaceholder}
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onFocus={() => setOpen(true)}
          aria-label="Search products"
          autoComplete="off"
        />
        <button type="submit" className="header-search-btn" aria-label="Search">
          🔍
        </button>
      </form>

      {open && (
        <div className="header-search-panel">
          {query.trim().length >= 2 && (
            <div className="header-search-section">
              <div className="header-search-label">{loadingSuggestions ? 'Searching...' : t.searchSuggestions}</div>
              {suggestions.length === 0 && !loadingSuggestions && (
                <div className="header-search-empty">No products match this keyword.</div>
              )}
              {suggestions.map((p) => (
                <button key={p.id} type="button" className="header-search-item" onClick={() => navigate(`/products/${p.id}`)}>
                  <span>{productName(p, language)}</span>
                  <span className="header-search-meta">{productCategoryName(p, language)}</span>
                </button>
              ))}
            </div>
          )}

          {history.length > 0 && (
            <div className="header-search-section">
              <div className="header-search-label">{t.searchRecent}</div>
              {history.map((term) => (
                <button key={term} type="button" className="header-search-item" onClick={() => goSearch(term)}>
                  <span>🕐 {term}</span>
                </button>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
