import { useCallback, useRef, useState } from 'react';
import { Link } from 'react-router';
import { likesApi } from './likes.api';
import { useResource } from '../../shared/lib/use-resource';
import { useErrors } from '../../shared/lib/use-errors';
import { useLanguage } from '../../shared/i18n/language-context';
import { useAuth } from '../auth/auth-context';
export function LikeButton({
  postId
}: {
  postId: string;
}) {
  const {
    words
  } = useLanguage();
  const {
    user
  } = useAuth();
  const report = useErrors();
  const loader = useCallback((signal: AbortSignal) => likesApi.get(postId, signal), [postId]);
  const resource = useResource(loader);
  const [busy, setBusy] = useState(false);
  const lock = useRef(false);
  async function toggle() {
    if (lock.current || !resource.data) return;
    lock.current = true;
    setBusy(true);
    try {
      await likesApi.set(postId, !resource.data.isLikedByMe);
      resource.reload();
    } catch (error) {
      report(error);
    } finally {
      lock.current = false;
      setBusy(false);
    }
  }
  if (resource.failed) return <button onClick={resource.reload}>{words.retry}</button>;
  if (!resource.data) return <span className="muted">{words.loading}</span>;
  return <div className="actions"><span>{resource.data.likeCount} ♥</span>{user ? <button aria-pressed={resource.data.isLikedByMe} disabled={busy} onClick={() => void toggle()}>{resource.data.isLikedByMe ? words.unlike : words.like}</button> : <Link to="/login">{words.like}</Link>}</div>;
}
