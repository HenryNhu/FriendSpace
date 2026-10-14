import { useCallback, useState } from 'react';
import { Link, useParams } from 'react-router';
import { useAuth } from '../../features/auth/auth-context';
import { profilesApi } from '../../features/profiles/profiles.api';
import { friendsApi } from '../../features/friends/friends.api';
import { FriendActions } from '../../features/friends/FriendActions';
import { PostCard } from '../../features/posts/components/PostCard';
import { useResource } from '../../shared/lib/use-resource';
import { useLanguage } from '../../shared/i18n/language-context';
import { Avatar } from '../../shared/ui/Avatar';
import { Pagination } from '../../shared/ui/Pagination';
export function ProfilePage() {
  const {
    userId = ''
  } = useParams();
  return <ProfileContent key={userId} userId={userId} />;
}
function ProfileContent({
  userId
}: {
  userId: string;
}) {
  const {
    user
  } = useAuth();
  const {
    words
  } = useLanguage();
  const [page, setPage] = useState(1);
  const loader = useCallback(async (signal: AbortSignal) => {
    const [profile, posts, relationship] = await Promise.all([profilesApi.getPublicProfile(userId, signal), profilesApi.getPosts(userId, page, signal), user ? friendsApi.withUser(userId, signal) : Promise.resolve(null)]);
    return {
      profile,
      posts,
      relationship
    };
  }, [userId, user, page]);
  const resource = useResource(loader);
  if (resource.loading) return <p role="status">{words.loading}</p>;
  if (!resource.data) return <section className="card"><p>{words.notFound}</p>{user?.id === userId && <Link to="/settings/profile">{words.completeProfile}</Link>}<button onClick={resource.reload}>{words.retry}</button></section>;
  const {
    profile,
    posts,
    relationship
  } = resource.data;
  return <><section className="card profile-summary"><Avatar name={profile.displayName} url={profile.avatarUrl} large /><h1>{profile.displayName}</h1><p className="content">{profile.bio}</p>
 {user?.id === userId ? <Link className="button" to="/settings/profile">{words.settings}</Link> : relationship && <FriendActions userId={userId} relationship={relationship} onChanged={resource.reload} />}
 </section><h2>{words.posts}</h2>{posts.length === 0 && <p className="card">{words.empty}</p>}{posts.map(post => <PostCard key={post.id} post={post} onChanged={resource.reload} />)}<Pagination page={page} count={posts.length} onChange={setPage} /></>;
}
