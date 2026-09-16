import { apiClient } from './client';
import type { Category, PagedResult, ProductDetail, ProductListItem, ProductReview } from '../types';

export interface ProductSearchParams {
  keyword?: string;
  categoryId?: string;
  minPrice?: number;
  maxPrice?: number;
  page?: number;
  pageSize?: number;
  sort?: string;
  minRating?: number;
}

export function searchProducts(params: ProductSearchParams) {
  return apiClient.get<PagedResult<ProductListItem>>('/products', { params }).then((r) => r.data);
}

export function getProductById(id: string) {
  return apiClient.get<ProductDetail>(`/products/${id}`).then((r) => r.data);
}

export function getCategories() {
  return apiClient.get<Category[]>('/categories').then((r) => r.data);
}

export interface CategoryInput {
  name: string;
  description: string | null;
  nameEn?: string | null;
  nameVi?: string | null;
  descriptionEn?: string | null;
  descriptionVi?: string | null;
}

export function createCategory(input: CategoryInput) {
  return apiClient.post<Category>('/categories', input).then((r) => r.data);
}

export function updateCategory(id: string, input: CategoryInput) {
  return apiClient.put(`/categories/${id}`, input);
}

export function deleteCategory(id: string) {
  return apiClient.delete(`/categories/${id}`);
}

export interface ProductInput {
  name: string;
  description: string | null;
  price: number;
  categoryId: string;
  nameEn?: string | null;
  nameVi?: string | null;
  descriptionEn?: string | null;
  descriptionVi?: string | null;
}

export function createProduct(input: ProductInput) {
  return apiClient.post<string>('/products', input).then((r) => r.data);
}

export function updateProduct(id: string, input: ProductInput) {
  return apiClient.put(`/products/${id}`, input);
}

export function deleteProduct(id: string) {
  return apiClient.delete(`/products/${id}`);
}

export function uploadProductImage(id: string, file: File) {
  const form = new FormData();
  form.append('file', file);
  return apiClient.post<string>(`/products/${id}/image`, form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
}

export function addProductGalleryImage(id: string, file: File) {
  const form = new FormData();
  form.append('file', file);
  return apiClient.post<string>(`/products/${id}/images`, form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
}

export function removeProductGalleryImage(id: string, imageId: string) {
  return apiClient.delete(`/products/${id}/images/${imageId}`);
}

export function getProductReviews(productId: string, page = 1, pageSize = 20) {
  return apiClient
    .get<PagedResult<ProductReview>>(`/products/${productId}/reviews`, { params: { page, pageSize } })
    .then((r) => r.data);
}

export function createProductReview(productId: string, rating: number, comment: string | null) {
  return apiClient.post<string>(`/products/${productId}/reviews`, { rating, comment }).then((r) => r.data);
}

export function deleteProductReview(productId: string, reviewId: string) {
  return apiClient.delete(`/products/${productId}/reviews/${reviewId}`);
}
