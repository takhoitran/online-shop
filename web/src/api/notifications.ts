import { apiClient } from './client';
import type { AppNotification, PagedResult } from '../types';

export function getNotifications(page = 1, pageSize = 20) {
  return apiClient
    .get<PagedResult<AppNotification>>('/notifications', { params: { page, pageSize } })
    .then((r) => r.data);
}

export function getUnreadNotificationCount() {
  return apiClient.get<number>('/notifications/unread-count').then((r) => r.data);
}

export function markNotificationAsRead(id: string) {
  return apiClient.post(`/notifications/${id}/read`);
}
