import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import * as authApi from '../api/auth';
import { clearSession, persistSession, readStoredUser, type CurrentUser } from './sessionStorage';

interface AuthContextValue {
  user: CurrentUser | null;
  login: (username: string, password: string) => Promise<void>;
  register: (username: string, password: string, fullName: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(readStoredUser);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      login: async (username, password) => {
        const result = await authApi.login(username, password);
        setUser(persistSession(result));
      },
      register: async (username, password, fullName) => {
        const result = await authApi.register(username, password, fullName);
        setUser(persistSession(result));
      },
      logout: () => {
        clearSession();
        setUser(null);
      },
    }),
    [user],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used inside an AuthProvider.');
  return context;
}
