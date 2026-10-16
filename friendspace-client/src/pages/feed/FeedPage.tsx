import { Link } from 'react-router';
import { useAuth } from '../../features/auth/auth-context';
import { useFeed } from '../../features/feed/use-feed';
import { feedMessages } from '../../features/feed/feed.messages';
import { PostEditor } from '../../features/posts/components/PostEditor';
import { PostCard } from '../../features/posts/components/PostCard';
import { postsApi } from '../../features/posts/posts.api';
import { useLanguage } from '../../shared/i18n/language-context';
import { useToast } from '../../shared/ui/toast/toast-context';

export default function FeedPage() {
  const { user, profile } = useAuth();
  const { language, words } = useLanguage();
  const { notify } = useToast();
  const feed = useFeed();
  const text = feedMessages[language];
  const refresh = () => { void feed.refresh(); };

  return <>
    <div className="heading">
      <h1>{words.feed}</h1>
      <button onClick={refresh} disabled={feed.loading}>{words.refresh}</button>
    </div>
    <p className="muted">{user ? text.personal : text.public}</p>

    {user && (profile ? <section className="card">
      <PostEditor onSave={async input => {
        await postsApi.createPost(input);
        notify(words.saved, 'success');
        // A refresh failure must not turn a successful write into a failed submission.
        refresh();
      }} />
    </section> : <p className="card"><Link to="/settings/profile">{words.completeProfile}</Link></p>)}

    {feed.hasNewPosts && <div className="card">
      <p role="status">{text.newPosts}</p>
      <button className="primary" onClick={refresh} disabled={feed.loading}>{text.showNew}</button>
    </div>}
    {feed.pollingFailed && <p role="status" className="muted">{text.pollPaused}</p>}
    {feed.loading && <p role="status">{words.loading}</p>}
    {feed.refreshFailed && <div className="card">
      <p>{text.refreshFailed}</p>
      <button onClick={refresh} disabled={feed.loading}>{words.retry}</button>
    </div>}

    {feed.initialized && !feed.loading && user && !feed.hasFriends &&
      <p className="card">{text.noFriends} <Link to="/people">{text.findFriends}</Link></p>}
    {feed.initialized && !feed.loading && feed.items.length === 0 &&
      <p className="card">{user ? text.emptyPersonal : text.emptyPublic}</p>}

    {feed.items.map(post => <PostCard key={post.id} post={post} onChanged={refresh} />)}

    {feed.initialized && feed.hasMore && <div className="form-actions">
      <button onClick={() => { void feed.loadMore(); }} disabled={feed.loading || feed.loadingMore}>
        {feed.loadingMore ? words.loading : feed.loadMoreFailed ? words.retry : text.loadMore}
      </button>
    </div>}
    {feed.initialized && !feed.hasMore && feed.items.length > 0 && !feed.loading &&
      <p className="muted">{text.end}</p>}
  </>;
}
