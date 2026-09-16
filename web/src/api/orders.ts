import { apiClient } from './client';
import type { Order, OrderStatus, OrderSummary, PagedResult, PaymentMethod } from '../types';

export interface CheckoutRequest {
  paymentMethod: PaymentMethod;
  recipientName: string;
  phoneNumber: string;
  addressLine: string;
  city: string;
  voucherCode?: string | null;
  productIds?: string[] | null;
}

export function checkout(request: CheckoutRequest) {
  return apiClient.post<string>('/orders/checkout', request).then((r) => r.data);
}

export interface OrderSearchParams {
  status?: OrderStatus | '';
  page?: number;
  pageSize?: number;
}

/** Buyer: only ever sees their own orders (backend enforces this regardless of params).
 * Seller/Admin: sees every order, optionally filtered by status — the Pending queue for
 * approval is `getOrders({ status: 'Pending' })`. */
export function getOrders(params: OrderSearchParams = {}) {
  return apiClient.get<PagedResult<OrderSummary>>('/orders', { params }).then((r) => r.data);
}

export function getOrderById(id: string) {
  return apiClient.get<Order>(`/orders/${id}`).then((r) => r.data);
}

export function cancelOrder(id: string, reason?: string) {
  return apiClient.post(`/orders/${id}/cancel`, { reason });
}

export function approveOrder(id: string) {
  return apiClient.post(`/orders/${id}/approve`);
}

export function markOrderPaid(id: string) {
  return apiClient.post(`/orders/${id}/mark-paid`);
}

export function shipOrder(id: string) {
  return apiClient.post(`/orders/${id}/ship`);
}

export function completeOrder(id: string) {
  return apiClient.post(`/orders/${id}/complete`);
}

export function returnOrder(id: string, reason?: string) {
  return apiClient.post(`/orders/${id}/return`, { reason });
}
