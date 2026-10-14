import { useId, useRef, useState } from 'react';
import { Link } from 'react-router';
import type { Post } from '../posts.types';
import { postsApi } from '../posts.api';
import { PostEditor } from './PostEditor';
import { useAuth } from '../../auth/auth-context';
import { LikeButton } from '../../likes/LikeButton';
import { Comments } from '../../comments/Comments';
import { Avatar } from '../../../shared/ui/Avatar';
import { ConfirmDialog } from '../../../shared/ui/ConfirmDialog';
import { useErrors } from '../../../shared/lib/use-errors';
import { useLanguage } from '../../../shared/i18n/language-context';
import { useToast } from '../../../shared/ui/toast/toast-context';
export function PostCard({
  post,
  onChanged,
  expanded = false
}: {
  post: Post;
  onChanged: () => void;
  expanded?: boolean;
}) {
  const {
    user
  } = useAuth();
  const {
    words,
    language
  } = useLanguage();
  const report = useErrors();
  const {
    notify
  } = useToast();
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [busy, setBusy] = useState(false);
  const lock = useRef(false);
  const menu = useRef<HTMLDetailsElement>(null);
  const [comments, setComments] = useState(expanded);
  const commentsId = useId();
  const name = post.authorDisplayName?.trim() || words.member;
  async function remove() {
    if (lock.current) return;
    lock.current = true;
    setBusy(true);
    try {
      await postsApi.deletePost(post.id);
      notify(words.deleted, 'success');
      setDeleting(false);
      onChanged();
    } catch (error) {
      report(error);
    } finally {
      lock.current = false;
      setBusy(false);
    }
  }
  return <article className="card post-card">
  <header className="post-header"><div className="person"><Link to={`/profiles/${post.authorId}`}><Avatar name={name} url={post.authorAvatarUrl} /></Link><div><Link className="person-name" to={`/profiles/${post.authorId}`}>{name}</Link><div className="muted"><Link to={`/posts/${post.id}`}><time dateTime={post.createdAt}>{new Date(post.createdAt).toLocaleString(language === 'vi' ? 'vi-VN' : 'en-US')}</time></Link>{post.updatedAt && ` · ${words.updated}`}</div><small className="muted">{[words.public, words.friendsOnly, words.onlyMe][post.visibility]}</small></div></div>
  {user?.id === post.authorId && !editing && <details ref={menu} className="post-menu"><summary aria-label={words.postActions}>•••</summary><div className="menu-items"><button onClick={() => {
            if (menu.current) menu.current.open = false;
            setEditing(true);
          }}>{words.edit}</button><button onClick={() => {
            if (menu.current) menu.current.open = false;
            setDeleting(true);
          }}>{words.remove}</button></div></details>}</header>
  {editing ? <PostEditor initial={post} onCancel={() => setEditing(false)} onSave={async input => {
      await postsApi.updatePost(post.id, input);
      setEditing(false);
      notify(words.saved, 'success');
      onChanged();
    }} /> : <>
   <p className="content">{post.content}</p>
   {!!post.images?.length && <div className="post-images">{post.images.map(image => <a key={image.id} href={image.url} target="_blank" rel="noreferrer"><img src={image.url} alt={words.postImage} loading="lazy" /></a>)}</div>}
  </>}
  <footer className="post-footer"><LikeButton postId={post.id} /><button aria-expanded={comments} aria-controls={commentsId} onClick={() => setComments(value => !value)}>{words.comments}</button><Link to={`/posts/${post.id}`}>{words.postDetail}</Link></footer>
  <div id={commentsId}>{comments && <Comments postId={post.id} postOwnerId={post.authorId} />}</div>
  {deleting && <ConfirmDialog title={words.deletePost} message={words.deletePostQuestion} busy={busy} onCancel={() => setDeleting(false)} onConfirm={() => void remove()} />}
 </article>;
}
