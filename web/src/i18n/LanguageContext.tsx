import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import { en } from './en';
import { vi } from './vi';

type Language = 'en' | 'vi';
type Copy = Record<keyof typeof en, string>;

interface LanguageContextValue {
  language: Language;
  t: Copy;
  toggleLanguage: () => void;
  setLanguage: (language: Language) => void;
}

const STORAGE_KEY = 'onlineshop_language';
const LanguageContext = createContext<LanguageContextValue | null>(null);

function readStoredLanguage(): Language {
  return localStorage.getItem(STORAGE_KEY) === 'vi' ? 'vi' : 'en';
}

export function LanguageProvider({ children }: { children: ReactNode }) {
  const [language, setLanguageState] = useState<Language>(readStoredLanguage);

  function setLanguage(next: Language) {
    setLanguageState(next);
    localStorage.setItem(STORAGE_KEY, next);
  }

  const value = useMemo<LanguageContextValue>(
    () => ({
      language,
      t: language === 'vi' ? vi : en,
      toggleLanguage: () => setLanguage(language === 'vi' ? 'en' : 'vi'),
      setLanguage,
    }),
    [language],
  );

  return <LanguageContext.Provider value={value}>{children}</LanguageContext.Provider>;
}

export function useLanguage(): LanguageContextValue {
  const context = useContext(LanguageContext);
  if (!context) throw new Error('useLanguage must be used inside LanguageProvider.');
  return context;
}
