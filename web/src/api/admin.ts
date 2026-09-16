import { apiClient } from './client';
import type { UserSummary } from '../types';

export function getUsers() {
  return apiClient.get<UserSummary[]>('/admin/users').then((r) => r.data);
}

export function createStaffUser(username: string, password: string, fullName: string, roleName: 'Seller' | 'Admin') {
  return apiClient.post<UserSummary>('/admin/users', { username, password, fullName, roleName }).then((r) => r.data);
}
