import { apiClient } from './client';
import type { Wishlist } from '../types';

export function getWishlist() {
  return apiClient.get<Wishlist>('/wishlist').then((r) => r.data);
}

export function addToWishlist(productId: string) {
  return apiClient.post(`/wishlist/items/${productId}`);
}

export function removeFromWishlist(productId: string) {
  return apiClient.delete(`/wishlist/items/${productId}`);
}
