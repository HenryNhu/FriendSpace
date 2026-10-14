import { useCallback, useEffect, useRef, useState, type ReactNode } from 'react';
import { authApi } from './auth.api';
import { AuthContext } from './auth-context';
import type { User } from './auth.types';
import { profilesApi } from '../profiles/profiles.api';
import type { Profile } from '../profiles/profiles.types';
import { ApiError, SESSION_INVALID, setCsrfToken } from '../../shared/api/http-client';
import { useToast } from '../../shared/ui/toast/toast-context';
import { useLanguage } from '../../shared/i18n/language-context';
import { messages } from '../../shared/i18n/messages';
async function readAccount() {
  const session = await authApi.getSession();
  let profile: Profile | null = null;
  if (session.user) {
    try {
      profile = await profilesApi.getProfile();
    } catch (error) {
      if (!(error instanceof ApiError && error.status === 404)) throw error;
    }
  }
  return {
    user: session.user,
    profile
  };
}
export function AuthProvider({
  children
}: {
  children: ReactNode;
}) {
  const [user, setUser] = useState<User | null>(null);
  const [profile, setProfile] = useState<Profile | null>(null);
  const [ready, setReady] = useState(false);
  const [failed, setFailed] = useState(false);
  const [revision, setRevision] = useState(0);
  const requestId = useRef(0);
  const recovering = useRef(false);
  const channel = useRef<BroadcastChannel | null>(null);
  const {
    notify
  } = useToast();
  const {
    words
  } = useLanguage();
  const refresh = useCallback(async () => {
    const id = ++requestId.current;
    setReady(false);
    setUser(null);
    setProfile(null);
    setFailed(false);
    setRevision(n => n + 1);
    try {
      const account = await readAccount();
      if (id !== requestId.current) return;
      setUser(account.user);
      setProfile(account.profile);
      setReady(true);
    } catch (error) {
      if (id === requestId.current) {
        setFailed(true);
        setCsrfToken(null);
      }
      throw error;
    }
  }, []);
  const announce = useCallback(() => channel.current?.postMessage('session-changed'), []);
  const signOut = useCallback(async () => {
    await authApi.logout();
    // Clear private component trees as soon as the server has removed the cookie.
    setReady(false);
    setUser(null);
    setProfile(null);
    setRevision(n => n + 1);
    announce();
    await refresh();
  }, [announce, refresh]);
  useEffect(() => {
    let active = true;
    const id = ++requestId.current;
    void readAccount().then(account => {
      if (!active || id !== requestId.current) return;
      setUser(account.user);
      setProfile(account.profile);
      setReady(true);
    }).catch(() => {
      if (!active || id !== requestId.current) return;
      setFailed(true);
      setCsrfToken(null);
      const language = document.documentElement.lang === 'en' ? 'en' : 'vi';
      notify(messages[language].connectionError, 'error');
    });
    return () => {
      active = false;
    };
  }, [notify]);
  useEffect(() => {
    const recover = async (event?: Event) => {
      if (recovering.current) return;
      recovering.current = true;
      const error = (event as CustomEvent<ApiError> | undefined)?.detail;
      if (error) notify(error.code === 'invalid_csrf' ? words.sessionChanged : words.sessionExpired, 'error');
      try {
        await refresh();
      } catch {
        notify(words.connectionError, 'error');
      } finally {
        recovering.current = false;
      }
    };
    const handle = (event: Event) => {
      void recover(event);
    };
    window.addEventListener(SESSION_INVALID, handle);
    if ('BroadcastChannel' in window) {
      channel.current = new BroadcastChannel('friendspace-session');
      channel.current.onmessage = () => {
        void recover();
      };
    }
    return () => {
      window.removeEventListener(SESSION_INVALID, handle);
      channel.current?.close();
      channel.current = null;
    };
  }, [refresh, notify, words]);
  return <AuthContext value={{
    user,
    profile,
    ready,
    failed,
    revision,
    refresh,
    signOut,
    announce,
    setProfile
  }}>{children}</AuthContext>;
}
