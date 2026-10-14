import { useCallback } from 'react';
import { Link, useParams } from 'react-router';
import { postsApi } from '../../features/posts/posts.api';
import { PostCard } from '../../features/posts/components/PostCard';
import { useResource } from '../../shared/lib/use-resource';
import { useLanguage } from '../../shared/i18n/language-context';
export function PostDetailPage() {
  const {
    postId = ''
  } = useParams();
  const {
    words
  } = useLanguage();
  const loader = useCallback((signal: AbortSignal) => postsApi.getPost(postId, signal), [postId]);
  const resource = useResource(loader);
  return <><h1>{words.postDetail}</h1><p><Link to="/">{words.backToFeed}</Link></p>{resource.loading && <p role="status">{words.loading}</p>}{resource.failed && <div className="card"><p>{words.notFound}</p><button onClick={resource.reload}>{words.retry}</button></div>}{resource.data && <PostCard key={postId} post={resource.data} onChanged={resource.reload} expanded />}</>;
}
