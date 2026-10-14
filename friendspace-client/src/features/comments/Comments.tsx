import { useCallback, useRef, useState } from 'react';
import { Link } from 'react-router';
import { commentsApi, type Comment } from './comments.api';
import { useAuth } from '../auth/auth-context';
import { useLanguage } from '../../shared/i18n/language-context';
import { useResource } from '../../shared/lib/use-resource';
import { useErrors } from '../../shared/lib/use-errors';
import { useToast } from '../../shared/ui/toast/toast-context';
import { Avatar } from '../../shared/ui/Avatar';
import { ConfirmDialog } from '../../shared/ui/ConfirmDialog';
import { Pagination } from '../../shared/ui/Pagination';
export function Comments({
  postId,
  postOwnerId
}: {
  postId: string;
  postOwnerId: string;
}) {
  const {
    user,
    profile
  } = useAuth();
  const {
    words,
    language
  } = useLanguage();
  const {
    notify
  } = useToast();
  const report = useErrors();
  const [page, setPage] = useState(1);
  const [content, setContent] = useState('');
  const [editing, setEditing] = useState<Comment | null>(null);
  const [deleting, setDeleting] = useState<Comment | null>(null);
  const [busy, setBusy] = useState(false);
  const lock = useRef(false);
  const loader = useCallback((signal: AbortSignal) => commentsApi.list(postId, page, signal), [postId, page]);
  const resource = useResource(loader);
  async function submit() {
    if (lock.current) return;
    if (!content.trim()) {
      notify(words.requiredField, 'error');
      return;
    }
    if (content.trim().length > 2000) {
      notify(words.tooLong, 'error');
      return;
    }
    lock.current = true;
    setBusy(true);
    try {
      if (editing) await commentsApi.update(postId, editing.id, content.trim());else await commentsApi.create(postId, content.trim());
      setEditing(null);
      setContent('');
      notify(words.saved, 'success');
      setPage(1);
      resource.reload();
    } catch (error) {
      report(error);
    } finally {
      lock.current = false;
      setBusy(false);
    }
  }
  async function remove() {
    if (!deleting || lock.current) return;
    lock.current = true;
    setBusy(true);
    try {
      await commentsApi.remove(postId, deleting.id);
      if (editing?.id === deleting.id) {
        setEditing(null);
        setContent('');
      }
      setDeleting(null);
      resource.reload();
      notify(words.deleted, 'success');
    } catch (error) {
      report(error);
    } finally {
      lock.current = false;
      setBusy(false);
    }
  }
  return <section className="comments"><h3>{words.comments}</h3>
  {user && profile ? <form noValidate onSubmit={event => {
      event.preventDefault();
      void submit();
    }}><label>{editing ? words.editComment : words.commentPlaceholder}<textarea rows={2} maxLength={2000} disabled={busy} value={content} onChange={event => setContent(event.target.value)} /></label><div className="form-actions"><button className="primary" disabled={busy}>{editing ? words.saveChanges : words.send}</button>{editing && <button type="button" disabled={busy} onClick={() => {
          setEditing(null);
          setContent('');
        }}>{words.cancel}</button>}</div></form> : <p><Link to={user ? '/settings/profile' : '/login'}>{user ? words.completeProfile : words.loginToInteract}</Link></p>}
  {resource.loading && <p role="status">{words.loading}</p>}
  {resource.failed && <button onClick={resource.reload}>{words.retry}</button>}
  {resource.data?.length === 0 && <p className="muted">{words.noComments}</p>}
  {resource.data?.map(comment => <article className="comment" key={comment.id}>
   <div className="person"><Avatar name={comment.authorDisplayName ?? words.member} url={comment.authorAvatarUrl} /><div><Link to={`/profiles/${comment.authorId}`}>{comment.authorDisplayName ?? words.member}</Link><div className="muted"><time dateTime={comment.createdAt}>{new Date(comment.createdAt).toLocaleString(language === 'vi' ? 'vi-VN' : 'en-US')}</time>{comment.updatedAt && ` · ${words.updated}`}</div></div></div>
   <p className="content">{comment.content}</p>
   <div className="actions">{user?.id === comment.authorId && <button disabled={busy} onClick={() => {
          setEditing(comment);
          setContent(comment.content);
        }}>{words.edit}</button>}{user && (user.id === comment.authorId || user.id === postOwnerId) && <button disabled={busy} onClick={() => setDeleting(comment)}>{words.remove}</button>}</div>
  </article>)}
  {resource.data && <Pagination page={page} count={resource.data.length} onChange={setPage} />}
  {deleting && <ConfirmDialog title={words.deleteComment} message={words.deleteCommentQuestion} busy={busy} onCancel={() => setDeleting(null)} onConfirm={() => void remove()} />}
 </section>;
}
