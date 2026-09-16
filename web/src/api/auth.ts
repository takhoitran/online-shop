import { apiClient } from './client';
import type { AuthResult, TelegramLinkCode } from '../types';

export function login(username: string, password: string) {
  return apiClient.post<AuthResult>('/auth/login', { username, password }).then((r) => r.data);
}

export function register(username: string, password: string, fullName: string) {
  return apiClient.post<AuthResult>('/auth/register', { username, password, fullName }).then((r) => r.data);
}

export function generateTelegramLinkCode() {
  return apiClient.post<TelegramLinkCode>('/auth/telegram-link/generate').then((r) => r.data);
}

export function changePassword(currentPassword: string, newPassword: string) {
  return apiClient.post('/auth/change-password', { currentPassword, newPassword });
}
