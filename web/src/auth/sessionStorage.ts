import type { AuthResult } from '../types';

export type CurrentUser = Pick<AuthResult, 'userId' | 'username' | 'fullName' | 'role'>;

const TOKEN_KEY = 'token';
const USER_KEY = 'user';

export function getStoredToken(): string | null {
  return sessionStorage.getItem(TOKEN_KEY) ?? localStorage.getItem(TOKEN_KEY);
}

export function readStoredUser(): CurrentUser | null {
  const raw = sessionStorage.getItem(USER_KEY) ?? localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as CurrentUser;
  } catch {
    return null;
  }
}

export function persistSession(result: AuthResult): CurrentUser {
  sessionStorage.setItem(TOKEN_KEY, result.token);
  localStorage.removeItem(TOKEN_KEY);

  const user: CurrentUser = {
    userId: result.userId,
    username: result.username,
    fullName: result.fullName,
    role: result.role,
  };
  sessionStorage.setItem(USER_KEY, JSON.stringify(user));
  localStorage.removeItem(USER_KEY);
  return user;
}

export function clearSession() {
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(USER_KEY);
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}
