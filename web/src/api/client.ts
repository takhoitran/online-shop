import axios from 'axios';
import type { ProblemDetails } from '../types';
import { clearSession, getStoredToken } from '../auth/sessionStorage';

export const apiClient = axios.create({
  baseURL: '/api',
});

apiClient.interceptors.request.use((config) => {
  const token = getStoredToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      clearSession();
    }
    return Promise.reject(error);
  },
);

/** Every command endpoint returns a ProblemDetails body on failure — surface .detail everywhere. */
export function extractErrorMessage(error: unknown, fallback = 'Something went wrong, please try again.'): string {
  if (axios.isAxiosError<ProblemDetails>(error)) {
    return error.response?.data?.detail ?? fallback;
  }
  return fallback;
}
