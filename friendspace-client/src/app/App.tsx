import { useEffect, useRef, useState } from 'react';
import { authApi } from '../features/auth/auth.api';
import { postsApi } from '../features/posts/posts.api';
import { profilesApi } from '../features/profiles/profiles.api';

import type { User } from '../features/auth/auth.types';
import type { Post } from '../features/posts/posts.types';
import type { Profile } from '../features/profiles/profiles.types';

import { ApiError } from '../shared/api/http-client';

import {
  messages,
  type Language,
} from '../shared/i18n/messages';

async function readAccount() {
  const session = await authApi.getSession();
  let profile: Profile | null = null;

  if (session.user) {
    try {
      profile = await profilesApi.getProfile();
    } catch (error) {
      if (!(error instanceof ApiError && error.status === 404)) {
        throw error;
      }
    }
  }

  return { user: session.user, profile };
}

export default function App() {
  const [language, setLanguage] = useState<Language>('vi');
  const words = messages[language];

  const [user, setUser] = useState<User | null>(null);
  const [profile, setProfile] = useState<Profile | null>(null);
  const [ready, setReady] = useState(false);

  const [posts, setPosts] = useState<Post[]>([]);
  const [page, setPage] = useState(1);
  const [content, setContent] = useState('');

  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [bio, setBio] = useState('');

  const [busy, setBusy] = useState(true);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const locked = useRef(true);

  useEffect(() => {
    document.documentElement.lang = language;
    document.title = 'FriendSpace';
  }, [language]);

  useEffect(() => {
    let active = true;

    Promise.all([readAccount(), postsApi.getPosts(1)])
      .then(([account, items]) => {
        if (!active) return;

        setUser(account.user);
        setProfile(account.profile);
        setDisplayName(account.profile?.displayName ?? '');
        setBio(account.profile?.bio ?? '');
        setPosts(items);
        setReady(true);
      })
      .catch(() => {
        if (active) {
          setError(messages.vi.connectionError);
        }
      })
      .finally(() => {
        if (active) {
          locked.current = false;
          setBusy(false);
        }
      });

    return () => {
      active = false;
    };
  }, []);

  async function loadAccount() {
    setReady(false);

    const account = await readAccount();

    setUser(account.user);
    setProfile(account.profile);
    setDisplayName(account.profile?.displayName ?? '');
    setBio(account.profile?.bio ?? '');
    setPassword('');
    setContent('');
    setReady(true);
  }

  async function loadPosts(targetPage: number) {
    const items = await postsApi.getPosts(targetPage);
    setPosts(items);
    setPage(targetPage);
  }

  async function run(action: () => Promise<void>) {
    if (locked.current) return;

    locked.current = true;
    setBusy(true);
    setError('');
    setNotice('');

    try {
      await action();
    } catch (caught) {
      if (
        caught instanceof ApiError &&
        (caught.status === 401 || caught.code === 'invalid_csrf')
      ) {
        setReady(false);
        setUser(null);
        setProfile(null);
        setPassword('');
        setContent('');
        setError(words.sessionChanged);
      } else {
        setError(
          caught instanceof ApiError
            ? caught.message
            : words.connectionError,
        );
      }
    } finally {
      locked.current = false;
      setBusy(false);
    }
  }

  async function authenticate() {
    if (mode === 'register') {
      await authApi.register(email.trim(), password);
      setMode('login');
      setNotice(words.registered);
    }

    await authApi.login(email.trim(), password);
    await loadAccount();
  }

  async function signOut() {
    await authApi.logout();

    setUser(null);
    setProfile(null);
    setContent('');
    setReady(false);

    await loadAccount();
  }

  async function saveProfile() {
    const saved = await profilesApi.saveProfile(displayName, bio);

    setProfile(saved);
    setDisplayName(saved.displayName);
    setBio(saved.bio);

    setPosts(current =>
      current.map(post =>
        post.authorId === saved.userId
          ? { ...post, authorDisplayName: saved.displayName }
          : post,
      ),
    );

    setNotice(words.saved);
  }

  async function publish() {
    const created = await postsApi.createPost(content);

    setContent('');
    setNotice(words.saved);

    if (page === 1) {
      setPosts(current => [created, ...current].slice(0, 10));
    } else {
      setPage(1);
      setPosts([created]);

      try {
        await loadPosts(1);
      } catch {
        setError(words.connectionError);
      }
    }
  }

  return (
    <div className="shell">
      <header className="topbar">
        <div className="brand">
          <span className="brand-mark">F</span>
          <div>
            <strong>FriendSpace</strong>
            <p>{words.tagline}</p>
          </div>
        </div>

        <div className="actions">
          <select
            aria-label="Language"
            value={language}
            onChange={event =>
              setLanguage(event.target.value as Language)
            }
          >
            <option value="vi">Tiếng Việt</option>
            <option value="en">English</option>
          </select>

          {ready && user && (
            <button
              type="button"
              disabled={busy}
              onClick={() => void run(signOut)}
            >
              {words.logout}
            </button>
          )}
        </div>
      </header>

      {error && <div className="alert error" role="alert">{error}</div>}
      {notice && <div className="alert success" role="status">{notice}</div>}

      <div className="layout">
        <aside>
          <section className="card">
            {!ready ? (
              <p>{busy ? words.loading : words.sessionPending}</p>
            ) : !user ? (
              <form
                onSubmit={event => {
                  event.preventDefault();
                  void run(authenticate);
                }}
              >
                <h2>{words[mode]}</h2>

                <fieldset disabled={busy}>
                  <label>
                    {words.email}
                    <input
                      type="email"
                      autoComplete="username"
                      required
                      value={email}
                      onChange={event => setEmail(event.target.value)}
                    />
                  </label>

                  <label>
                    {words.password}
                    <input
                      type="password"
                      autoComplete={
                        mode === 'register'
                          ? 'new-password'
                          : 'current-password'
                      }
                      required
                      minLength={mode === 'register' ? 8 : undefined}
                      value={password}
                      onChange={event => setPassword(event.target.value)}
                    />
                  </label>

                  {mode === 'register' && (
                    <p className="muted">{words.passwordHint}</p>
                  )}

                  <button className="primary full" type="submit">
                    {words[mode]}
                  </button>

                  <button
                    className="full"
                    type="button"
                    onClick={() =>
                      setMode(mode === 'login' ? 'register' : 'login')
                    }
                  >
                    {mode === 'login' ? words.register : words.login}
                  </button>
                </fieldset>
              </form>
            ) : (
              <form
                onSubmit={event => {
                  event.preventDefault();
                  void run(saveProfile);
                }}
              >
                <h2>{words.profile}</h2>
                <p className="muted account-email">{user.email}</p>

                <fieldset disabled={busy}>
                  <label>
                    {words.displayName}
                    <input
                      required
                      minLength={2}
                      maxLength={80}
                      value={displayName}
                      onChange={event => setDisplayName(event.target.value)}
                    />
                  </label>

                  <label>
                    {words.bio}
                    <textarea
                      rows={4}
                      maxLength={500}
                      value={bio}
                      onChange={event => setBio(event.target.value)}
                    />
                  </label>

                  <button
                    className="primary full"
                    type="submit"
                    disabled={displayName.trim().length < 2}
                  >
                    {words.save}
                  </button>
                </fieldset>
              </form>
            )}
          </section>
        </aside>

        <main>
          <div className="section-heading">
            <h1>{words.feed}</h1>
            <button
              type="button"
              disabled={busy}
              onClick={() =>
                void run(async () => {
                  if (!ready) await loadAccount();
                  await loadPosts(page);
                })
              }
            >
              {words.refresh}
            </button>
          </div>

          {ready && user && (
            <section className="card">
              {profile ? (
                <form
                  onSubmit={event => {
                    event.preventDefault();
                    void run(publish);
                  }}
                >
                  <label className="composer-label">
                    {profile.displayName}
                    <textarea
                      rows={3}
                      placeholder={words.placeholder}
                      value={content}
                      disabled={busy}
                      onChange={event => setContent(event.target.value)}
                    />
                  </label>

                  <div className="composer-actions">
                    <button
                      className="primary"
                      type="submit"
                      disabled={busy || !content.trim()}
                    >
                      {words.publish}
                    </button>
                  </div>
                </form>
              ) : (
                <p>{words.completeProfile}</p>
              )}
            </section>
          )}

          {busy && <p className="muted" role="status">{words.loading}</p>}

          {!busy && posts.length === 0 && (
            <section className="card empty">{words.empty}</section>
          )}

          {posts.map(post => (
            <article className="card post" key={post.id}>
              <div className="post-header">
                <div className="avatar">
                  {Array.from(post.authorDisplayName || 'F')[0]}
                </div>
                <div>
                  <strong>{post.authorDisplayName || words.member}</strong>
                  <time dateTime={post.createdAt}>
                    {new Date(post.createdAt).toLocaleString(
                      language === 'vi' ? 'vi-VN' : 'en-US',
                    )}
                  </time>
                </div>
              </div>

              <p className="post-content">{post.content}</p>
            </article>
          ))}

          <nav className="pagination" aria-label={words.page}>
            <button
              disabled={busy || page === 1}
              onClick={() => void run(() => loadPosts(page - 1))}
            >
              {words.previous}
            </button>

            <span>{words.page} {page}</span>

            <button
              disabled={busy || posts.length < 10}
              onClick={() => void run(() => loadPosts(page + 1))}
            >
              {words.next}
            </button>
          </nav>
        </main>
      </div>
    </div>
  );
}