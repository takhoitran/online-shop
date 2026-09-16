import { apiClient } from './client';
import type { Cart } from '../types';

export function getCart() {
  return apiClient.get<Cart>('/cart').then((r) => r.data);
}

export function addCartItem(productId: string, quantity: number) {
  return apiClient.post('/cart/items', { productId, quantity });
}

export function updateCartItemQuantity(productId: string, quantity: number) {
  return apiClient.put(`/cart/items/${productId}`, { quantity });
}

export function removeCartItem(productId: string) {
  return apiClient.delete(`/cart/items/${productId}`);
}
