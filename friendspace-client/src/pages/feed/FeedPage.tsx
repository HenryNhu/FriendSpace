import { useCallback, useState } from 'react';
import { Link } from 'react-router';
import { useAuth } from '../../features/auth/auth-context';
import { PostEditor } from '../../features/posts/components/PostEditor';
import { PostCard } from '../../features/posts/components/PostCard';
import { postsApi } from '../../features/posts/posts.api';
import { useResource } from '../../shared/lib/use-resource';
import { useLanguage } from '../../shared/i18n/language-context';
import { useToast } from '../../shared/ui/toast/toast-context';
import { Pagination } from '../../shared/ui/Pagination';
export default function FeedPage() {
  const {
    user,
    profile
  } = useAuth();
  const {
    words
  } = useLanguage();
  const {
    notify
  } = useToast();
  const [page, setPage] = useState(1);
  const loader = useCallback((signal: AbortSignal) => postsApi.getPosts(page, signal), [page]);
  const resource = useResource(loader);
  return <><div className="heading"><h1>{words.feed}</h1><button onClick={resource.reload} disabled={resource.loading}>{words.refresh}</button></div>
 {user && (profile ? <section className="card"><PostEditor onSave={async input => {
        await postsApi.createPost(input);
        notify(words.saved, 'success');
        setPage(1);
        resource.reload();
      }} /></section> : <p className="card"><Link to="/settings/profile">{words.completeProfile}</Link></p>)}
 {resource.loading && <p role="status">{words.loading}</p>}
 {resource.failed && <button onClick={resource.reload}>{words.retry}</button>}
 {resource.data?.length === 0 && <p className="card">{words.empty}</p>}
 {resource.data?.map(post => <PostCard key={post.id} post={post} onChanged={resource.reload} />)}
 {resource.data && <Pagination page={page} count={resource.data.length} onChange={setPage} />}
 </>;
}
