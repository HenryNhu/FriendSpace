import { createContext, useContext } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { User } from './auth.types';
import type { Profile } from '../profiles/profiles.types';
export interface AuthValue {
  user: User | null;
  profile: Profile | null;
  ready: boolean;
  failed: boolean;
  revision: number;
  refresh: () => Promise<void>;
  signOut: () => Promise<void>;
  announce: () => void;
  setProfile: Dispatch<SetStateAction<Profile | null>>;
}
export const AuthContext = createContext<AuthValue | null>(null);
export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('AuthProvider is missing');
  return context;
}
