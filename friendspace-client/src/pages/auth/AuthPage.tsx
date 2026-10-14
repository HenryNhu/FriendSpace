import { useRef, useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router';
import { AuthForm } from '../../features/auth/components/AuthForm';
import { authApi } from '../../features/auth/auth.api';
import { useAuth } from '../../features/auth/auth-context';
import { ApiError } from '../../shared/api/http-client';
import { useLanguage } from '../../shared/i18n/language-context';
import { useErrors } from '../../shared/lib/use-errors';
import { useToast } from '../../shared/ui/toast/toast-context';
export function AuthPage({
  mode
}: {
  mode: 'login' | 'register';
}) {
  const {
    words
  } = useLanguage();
  const {
    user,
    refresh,
    announce
  } = useAuth();
  const {
    notify
  } = useToast();
  const report = useErrors();
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [busy, setBusy] = useState(false);
  const lock = useRef(false);
  const from = (location.state as {
    from?: string;
  } | null)?.from;
  const destination = from?.startsWith('/') && !from.startsWith('//') && !['/login', '/register'].includes(from) ? from : '/';
  if (user) return <Navigate to={destination} replace />;
  async function submit() {
    if (lock.current) return;
    lock.current = true;
    setBusy(true);
    try {
      if (mode === 'register') {
        await authApi.register(email.trim(), password);
        setPassword('');
        notify(words.registered, 'success');
        navigate('/login', {
          replace: true,
          state: location.state
        });
        return;
      }
      try {
        await authApi.login(email.trim(), password);
      } catch (error) {
        if (error instanceof ApiError && error.status === 401) {
          const messages: Record<string, string> = {
            Failed: words.invalidCredentials,
            LockedOut: words.accountLocked,
            NotAllowed: words.loginNotAllowed,
            RequiresTwoFactor: words.twoFactorRequired
          };
          notify(messages[error.message] ?? words.invalidCredentials, 'error');
          setPassword('');
          return;
        }
        throw error;
      }
      announce();
      await refresh();
      notify(words.loggedIn, 'success');
      navigate(destination, {
        replace: true
      });
    } catch (error) {
      if (error instanceof ApiError && error.code === 'invalid_csrf') {
        notify(words.sessionChanged, 'error');
        await refresh().catch(report);
      } else report(error);
    } finally {
      lock.current = false;
      setBusy(false);
    }
  }
  return <div className="auth-page"><AuthForm words={words} mode={mode} email={email} password={password} busy={busy} onEmailChange={setEmail} onPasswordChange={setPassword} onModeChange={value => {
      setPassword('');
      navigate(`/${value}`, {
        state: location.state
      });
    }} onSubmit={() => void submit()} /></div>;
}
