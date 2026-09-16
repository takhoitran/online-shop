import { apiClient } from './client';
import type { DiscountType, PublicVoucher, Voucher, VoucherPreview } from '../types';

export function getPublicVouchers() {
  return apiClient.get<PublicVoucher[]>('/vouchers/public').then((r) => r.data);
}

export function getVouchers() {
  return apiClient.get<Voucher[]>('/vouchers').then((r) => r.data);
}

export interface CreateVoucherInput {
  code: string;
  discountType: DiscountType;
  discountValue: number;
  maxUses: number | null;
  expiresAtUtc: string | null;
}

export function createVoucher(input: CreateVoucherInput) {
  return apiClient.post<string>('/vouchers', input).then((r) => r.data);
}

export interface UpdateVoucherInput {
  discountType: DiscountType;
  discountValue: number;
  maxUses: number | null;
  expiresAtUtc: string | null;
}

export function updateVoucher(id: string, input: UpdateVoucherInput) {
  return apiClient.put(`/vouchers/${id}`, input);
}

export function deactivateVoucher(id: string) {
  return apiClient.post(`/vouchers/${id}/deactivate`);
}

export function previewVoucher(code: string, productIds?: string[]) {
  return apiClient
    .get<VoucherPreview>('/vouchers/preview', {
      params: { code, productIds },
      // ASP.NET Core's query-string binder expects repeated `productIds=a&productIds=b`,
      // not axios's default `productIds[]=a&productIds[]=b`.
      paramsSerializer: { indexes: null },
    })
    .then((r) => r.data);
}
