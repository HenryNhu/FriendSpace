import { Fragment, useRef, useState, type ReactNode } from 'react';
import { Link, NavLink, useNavigate } from 'react-router';
import { useAuth } from '../features/auth/auth-context';
import { useLanguage } from '../shared/i18n/language-context';
import type { Language } from '../shared/i18n/messages';
import { useErrors } from '../shared/lib/use-errors';
import { useToast } from '../shared/ui/toast/toast-context';
export function AppLayout({
  children
}: {
  children: ReactNode;
}) {
  const {
    user,
    ready,
    failed,
    revision,
    refresh,
    signOut
  } = useAuth();
  const {
    words,
    language,
    setLanguage
  } = useLanguage();
  const report = useErrors();
  const {
    notify
  } = useToast();
  const [busy, setBusy] = useState(false);
  const lock = useRef(false);
  const navigate = useNavigate();
  async function logout() {
    if (lock.current) return;
    lock.current = true;
    setBusy(true);
    try {
      await signOut();
      notify(words.loggedOut, 'success');
      navigate('/', {
        replace: true
      });
    } catch (error) {
      report(error);
    } finally {
      lock.current = false;
      setBusy(false);
    }
  }
  return <div className="shell"><header className="topbar"><Link to="/" className="brand"><span className="brand-mark">F</span><div><strong>FriendSpace</strong><small>{words.tagline}</small></div></Link><div className="actions"><select aria-label={words.language} value={language} onChange={event => setLanguage(event.target.value as Language)}><option value="vi">Tiếng Việt</option><option value="en">English</option></select>{ready && (user ? <button disabled={busy} onClick={() => void logout()}>{words.logout}</button> : <><Link className="button" to="/login">{words.login}</Link><Link className="button primary" to="/register">{words.register}</Link></>)}</div></header>
 <nav className="main-nav" aria-label="FriendSpace"><NavLink to="/" end>{words.feed}</NavLink><NavLink to="/people">{words.people}</NavLink>{ready && user && <><NavLink to="/friends">{words.friends}</NavLink><NavLink to={`/profiles/${user.id}`}>{words.profile}</NavLink><NavLink to="/settings/profile">{words.settings}</NavLink></>}</nav>
 <main className="app-main">{!ready ? <section className="card"><p role="status">{failed ? words.connectionError : words.loading}</p>{failed && <button onClick={() => {
          void refresh().catch(report);
        }}>{words.sessionRetry}</button>}</section> : <Fragment key={revision}>{children}</Fragment>}</main>
 </div>;
}
