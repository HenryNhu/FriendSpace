import { useRef, useState } from 'react';
import { useAuth } from '../../features/auth/auth-context';
import { profilesApi } from '../../features/profiles/profiles.api';
import { ProfileForm } from '../../features/profiles/components/ProfileForm';
import { AvatarEditor } from '../../features/media/AvatarEditor';
import { useLanguage } from '../../shared/i18n/language-context';
import { useErrors } from '../../shared/lib/use-errors';
import { useToast } from '../../shared/ui/toast/toast-context';
export function ProfileSettingsPage() {
  const {
    user,
    profile,
    setProfile
  } = useAuth();
  const {
    words
  } = useLanguage();
  const report = useErrors();
  const {
    notify
  } = useToast();
  const [displayName, setDisplayName] = useState(profile?.displayName ?? '');
  const [bio, setBio] = useState(profile?.bio ?? '');
  const [busy, setBusy] = useState(false);
  const lock = useRef(false);
  async function save() {
    if (lock.current) return;
    lock.current = true;
    setBusy(true);
    try {
      const saved = await profilesApi.saveProfile(displayName.trim(), bio.trim());
      setProfile(saved);
      setDisplayName(saved.displayName);
      setBio(saved.bio);
      notify(words.saved, 'success');
    } catch (error) {
      report(error);
    } finally {
      lock.current = false;
      setBusy(false);
    }
  }
  return <><h1>{words.settings}</h1><ProfileForm words={words} email={user?.email ?? null} displayName={displayName} bio={bio} busy={busy} onDisplayNameChange={setDisplayName} onBioChange={setBio} onSubmit={() => void save()} /><AvatarEditor /></>;
}
