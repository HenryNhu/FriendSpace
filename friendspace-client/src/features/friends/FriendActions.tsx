import { useRef, useState } from 'react';
import { friendsApi, type Relationship } from './friends.api';
import { useLanguage } from '../../shared/i18n/language-context';
import { useErrors } from '../../shared/lib/use-errors';
import { useToast } from '../../shared/ui/toast/toast-context';
import { ConfirmDialog } from '../../shared/ui/ConfirmDialog';
export function FriendActions({
  userId,
  relationship,
  onChanged
}: {
  userId: string;
  relationship: Relationship;
  onChanged: () => void;
}) {
  const {
    words
  } = useLanguage();
  const report = useErrors();
  const {
    notify
  } = useToast();
  const [busy, setBusy] = useState(false);
  const lock = useRef(false);
  const [confirm, setConfirm] = useState(false);
  async function act(action: 'invite' | 'accept' | 'remove') {
    if (lock.current) return;
    lock.current = true;
    setBusy(true);
    try {
      if (action === 'invite') await friendsApi.invite(userId);else if (relationship.id) {
        if (action === 'accept') await friendsApi.accept(relationship.id);else await friendsApi.remove(relationship.id);
      }
      setConfirm(false);
      notify(words.friendshipSaved, 'success');
      onChanged();
    } catch (error) {
      report(error);
      onChanged();
    } finally {
      lock.current = false;
      setBusy(false);
    }
  }
  if (relationship.state === 'self') return null;
  return <div className="actions">
  {relationship.state === 'none' && <button className="primary" disabled={busy} onClick={() => void act('invite')}>{words.addFriend}</button>}
  {relationship.state === 'incoming' && <><button className="primary" disabled={busy} onClick={() => void act('accept')}>{words.accept}</button><button disabled={busy} onClick={() => void act('remove')}>{words.reject}</button></>}
  {relationship.state === 'outgoing' && <button disabled={busy} onClick={() => void act('remove')}>{words.cancelRequest}</button>}
  {relationship.state === 'accepted' && <button disabled={busy} onClick={() => setConfirm(true)}>{words.unfriend}</button>}
  {confirm && <ConfirmDialog title={words.unfriend} message={words.unfriendQuestion} busy={busy} onCancel={() => setConfirm(false)} onConfirm={() => void act('remove')} />}
 </div>;
}
