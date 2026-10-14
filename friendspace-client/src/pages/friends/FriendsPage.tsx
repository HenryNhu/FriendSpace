import { useCallback } from 'react';
import { Link } from 'react-router';
import { friendsApi } from '../../features/friends/friends.api';
import { FriendActions } from '../../features/friends/FriendActions';
import { useResource } from '../../shared/lib/use-resource';
import { useLanguage } from '../../shared/i18n/language-context';
import { Avatar } from '../../shared/ui/Avatar';
export function FriendsPage() {
  const {
    words
  } = useLanguage();
  const loader = useCallback((signal: AbortSignal) => friendsApi.list(signal), []);
  const resource = useResource(loader);
  return <><div className="heading"><h1>{words.friends}</h1><button onClick={resource.reload} disabled={resource.loading}>{words.refresh}</button></div>
 {resource.loading && <p role="status">{words.loading}</p>}{resource.failed && <button onClick={resource.reload}>{words.retry}</button>}{resource.data?.length === 0 && <p className="card">{words.noFriends}</p>}
 {(['incoming', 'outgoing', 'accepted'] as const).map(state => <section key={state}><h2>{words[state]}</h2>{resource.data?.filter(friend => friend.state === state).map(friend => <article className="card friend-row" key={friend.id}><div className="person"><Avatar name={friend.displayName || words.member} url={friend.avatarUrl} /><Link to={`/profiles/${friend.otherUserId}`}>{friend.displayName || words.member}</Link></div><FriendActions userId={friend.otherUserId} relationship={friend} onChanged={resource.reload} /></article>)}</section>)}
 </>;
}
