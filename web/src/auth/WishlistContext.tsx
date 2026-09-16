import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import * as wishlistApi from '../api/wishlist';
import type { Wishlist } from '../types';
import { useAuth } from './AuthContext';

interface WishlistContextValue {
  wishlist: Wishlist | null;
  productIds: Set<string>;
  loading: boolean;
  refresh: () => Promise<void>;
  isWishlisted: (productId: string) => boolean;
  toggle: (productId: string) => Promise<void>;
}

const WishlistContext = createContext<WishlistContextValue | null>(null);

export function WishlistProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const [wishlist, setWishlist] = useState<Wishlist | null>(null);
  const [loading, setLoading] = useState(false);

  const refresh = useCallback(async () => {
    if (!user || user.role !== 'Buyer') {
      setWishlist(null);
      return;
    }
    setLoading(true);
    try {
      setWishlist(await wishlistApi.getWishlist());
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const productIds = useMemo(() => new Set(wishlist?.items.map((i) => i.productId) ?? []), [wishlist]);

  const value = useMemo<WishlistContextValue>(
    () => ({
      wishlist,
      productIds,
      loading,
      refresh,
      isWishlisted: (productId) => productIds.has(productId),
      toggle: async (productId) => {
        if (productIds.has(productId)) {
          await wishlistApi.removeFromWishlist(productId);
        } else {
          await wishlistApi.addToWishlist(productId);
        }
        await refresh();
      },
    }),
    [wishlist, productIds, loading, refresh],
  );

  return <WishlistContext.Provider value={value}>{children}</WishlistContext.Provider>;
}

export function useWishlist(): WishlistContextValue {
  const context = useContext(WishlistContext);
  if (!context) throw new Error('useWishlist must be used inside a WishlistProvider.');
  return context;
}
