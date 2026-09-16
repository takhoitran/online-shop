import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import * as cartApi from '../api/cart';
import type { Cart } from '../types';
import { useAuth } from './AuthContext';

interface CartContextValue {
  cart: Cart | null;
  itemCount: number;
  loading: boolean;
  refresh: (options?: { silent?: boolean }) => Promise<void>;
  addItem: (productId: string, quantity: number) => Promise<void>;
  updateItemQuantity: (productId: string, quantity: number) => Promise<void>;
  removeItem: (productId: string) => Promise<void>;
}

const CartContext = createContext<CartContextValue | null>(null);

export function CartProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const [cart, setCart] = useState<Cart | null>(null);
  const [loading, setLoading] = useState(() => Boolean(user && user.role === 'Buyer'));

  const refresh = useCallback(async (options?: { silent?: boolean }) => {
    if (!user || user.role !== 'Buyer') {
      setCart(null);
      return;
    }
    if (!options?.silent) {
      setLoading(true);
    }
    try {
      setCart(await cartApi.getCart());
    } finally {
      if (!options?.silent) {
        setLoading(false);
      }
    }
  }, [user]);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const value = useMemo<CartContextValue>(
    () => ({
      cart,
      itemCount: cart?.items.reduce((sum, i) => sum + i.quantity, 0) ?? 0,
      loading,
      refresh,
      addItem: async (productId, quantity) => {
        await cartApi.addCartItem(productId, quantity);
        await refresh({ silent: true });
      },
      updateItemQuantity: async (productId, quantity) => {
        await cartApi.updateCartItemQuantity(productId, quantity);
        await refresh({ silent: true });
      },
      removeItem: async (productId) => {
        await cartApi.removeCartItem(productId);
        await refresh({ silent: true });
      },
    }),
    [cart, loading, refresh],
  );

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart(): CartContextValue {
  const context = useContext(CartContext);
  if (!context) throw new Error('useCart must be used inside a CartProvider.');
  return context;
}
