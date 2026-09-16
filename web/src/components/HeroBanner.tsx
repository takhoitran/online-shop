import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import type { Category } from '../types';
import { useLanguage } from '../i18n/LanguageContext';

interface Slide {
  eyebrow: Record<'en' | 'vi', string>;
  title: Record<'en' | 'vi', string>;
  subtitle: Record<'en' | 'vi', string>;
  ctaLabel: Record<'en' | 'vi', string>;
  categoryName: string;
  gradientClass: 'gradient-a' | 'gradient-b' | 'gradient-c';
}

const SLIDES: Slide[] = [
  {
    eyebrow: { en: 'New Season', vi: 'Mua Sam Moi' },
    title: { en: 'Fresh Styles Just Dropped', vi: 'Phong Cach Moi Vua Len Ke' },
    subtitle: {
      en: 'Discover the latest tees and hoodies made for everyday comfort.',
      vi: 'Kham pha ao thun va hoodie moi, de mac dep moi ngay.',
    },
    ctaLabel: { en: 'Shop T-Shirts', vi: 'Mua Ao Thun' },
    categoryName: 'T-Shirts',
    gradientClass: 'gradient-a',
  },
  {
    eyebrow: { en: 'Step Up', vi: 'Nang Cap Buoc Chan' },
    title: { en: 'Gear Built To Perform', vi: 'Giay Dep Cho Moi Chuyen Dong' },
    subtitle: {
      en: 'From the court to the street - find your next favorite pair.',
      vi: 'Tu san tap den duong pho, tim doi giay tiep theo cua ban.',
    },
    ctaLabel: { en: 'Shop Shoes', vi: 'Mua Giay' },
    categoryName: 'Shoes',
    gradientClass: 'gradient-b',
  },
  {
    eyebrow: { en: 'Tech Essentials', vi: 'Do Cong Nghe Thiet Yeu' },
    title: { en: 'Smarter Everyday Carry', vi: 'Tien Ich Hon Moi Ngay' },
    subtitle: {
      en: 'Watches, speakers, and accessories to keep you connected.',
      vi: 'Dong ho, loa va phu kien giup ban luon ket noi.',
    },
    ctaLabel: { en: 'Shop Electronics', vi: 'Mua Do Dien Tu' },
    categoryName: 'Electronics',
    gradientClass: 'gradient-c',
  },
];

export function HeroBanner({ categories }: { categories: Category[] }) {
  const { language } = useLanguage();
  const [index, setIndex] = useState(0);

  useEffect(() => {
    const timer = setInterval(() => setIndex((i) => (i + 1) % SLIDES.length), 5500);
    return () => clearInterval(timer);
  }, []);

  function goTo(i: number) {
    setIndex((i + SLIDES.length) % SLIDES.length);
  }

  return (
    <div className="hero-banner">
      {SLIDES.map((slide, i) => {
        const category = categories.find((c) => c.name === slide.categoryName);
        return (
          <div key={slide.categoryName} className={`hero-slide ${slide.gradientClass} ${i === index ? 'active' : ''}`}>
            <div className="hero-slide-overlay" />
            <div className="hero-slide-content">
              <div className="hero-eyebrow">{slide.eyebrow[language]}</div>
              <h1>{slide.title[language]}</h1>
              <p>{slide.subtitle[language]}</p>
              <Link to={category ? `/categories/${category.id}` : '/products'} className="hero-cta">
                {slide.ctaLabel[language]}
              </Link>
            </div>
          </div>
        );
      })}

      <button className="hero-arrow prev" onClick={() => goTo(index - 1)} aria-label="Previous slide">
        ‹
      </button>
      <button className="hero-arrow next" onClick={() => goTo(index + 1)} aria-label="Next slide">
        ›
      </button>

      <div className="hero-dots">
        {SLIDES.map((_, i) => (
          <button
            key={i}
            className={`hero-dot ${i === index ? 'active' : ''}`}
            onClick={() => goTo(i)}
            aria-label={`Go to slide ${i + 1}`}
          />
        ))}
      </div>
    </div>
  );
}
