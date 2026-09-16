import { apiClient } from './client';
import type { RestockReport } from '../types';

export function askProductAdvisor(message: string) {
  return apiClient.post<string>('/ai/ask', { message }).then((r) => r.data);
}

export function getRestockReport() {
  return apiClient.post<RestockReport>('/ai/restock-report').then((r) => r.data);
}
