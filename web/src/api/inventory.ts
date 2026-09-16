import { apiClient } from './client';
import type { ProductStock } from '../types';

export function getProductStock(productId: string) {
  return apiClient.get<ProductStock>(`/inventory/${productId}`).then((r) => r.data);
}

export function receiveStock(productId: string, quantity: number, note: string | null) {
  return apiClient.post('/inventory/receive', { productId, quantity, note });
}

export function issueStock(productId: string, quantity: number, note: string) {
  return apiClient.post('/inventory/issue', { productId, quantity, note });
}
