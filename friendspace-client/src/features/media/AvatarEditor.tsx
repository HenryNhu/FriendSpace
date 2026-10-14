import { useEffect, useRef, useState } from 'react';
import { useAuth } from '../auth/auth-context';
import { profilesApi } from '../profiles/profiles.api';
import { imageTypes, maxImageBytes, uploadImage } from './media.api';
import { Avatar } from '../../shared/ui/Avatar';
import { useLanguage } from '../../shared/i18n/language-context';
import { useToast } from '../../shared/ui/toast/toast-context';
import { useErrors } from '../../shared/lib/use-errors';
export function AvatarEditor() {
  const {
    profile,
    setProfile
  } = useAuth();
  const {
    words
  } = useLanguage();
  const {
    notify
  } = useToast();
  const report = useErrors();
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [progress, setProgress] = useState(0);
  const [busy, setBusy] = useState(false);
  const lock = useRef(false);
  const url = useRef<string | null>(null);
  const abort = useRef<AbortController | null>(null);
  useEffect(() => () => {
    if (url.current) URL.revokeObjectURL(url.current);
    abort.current?.abort();
  }, []);
  function clear() {
    if (url.current) URL.revokeObjectURL(url.current);
    url.current = null;
    setPreview(null);
    setFile(null);
  }
  function choose(value?: File) {
    if (!value) return;
    if (!imageTypes.includes(value.type)) {
      notify(words.invalidImage, 'error');
      return;
    }
    if (value.size > maxImageBytes) {
      notify(words.imageTooLarge, 'error');
      return;
    }
    clear();
    const next = URL.createObjectURL(value);
    url.current = next;
    setPreview(next);
    setFile(value);
    setProgress(0);
  }
  async function upload() {
    if (!file || !profile || lock.current) return;
    lock.current = true;
    setBusy(true);
    abort.current = new AbortController();
    try {
      const media = await uploadImage(file, setProgress, abort.current.signal, true);
      setProfile(current => current?.userId === profile.userId ? {
        ...current,
        avatarUrl: media.url
      } : current);
      clear();
      notify(words.saved, 'success');
    } catch (error) {
      report(error);
    } finally {
      lock.current = false;
      setBusy(false);
    }
  }
  async function remove() {
    if (!profile || lock.current) return;
    lock.current = true;
    setBusy(true);
    try {
      await profilesApi.removeAvatar();
      setProfile(current => current?.userId === profile.userId ? {
        ...current,
        avatarUrl: null
      } : current);
      clear();
      notify(words.deleted, 'success');
    } catch (error) {
      report(error);
    } finally {
      lock.current = false;
      setBusy(false);
    }
  }
  if (!profile) return null;
  return <section className="card"><h2>{words.avatar}</h2><Avatar name={profile.displayName} url={preview ?? profile.avatarUrl} large />
 <label>{words.chooseAvatar}<input type="file" accept="image/jpeg,image/png,image/webp" disabled={busy} onChange={event => {
        choose(event.target.files?.[0]);
        event.target.value = '';
      }} /></label>
 <p className="muted">JPEG, PNG, WebP · 5 MB</p>
 {busy && file && <div role="status"><progress max={100} value={progress} aria-label={words.uploadProgress} /><span>{progress === 100 ? words.processingImage : `${progress}%`}</span></div>}
 <div className="form-actions">{file && <><button className="primary" disabled={busy} onClick={() => void upload()}>{words.changeAvatar}</button><button disabled={busy} onClick={clear}>{words.cancel}</button></>}{profile.avatarUrl && <button disabled={busy} onClick={() => void remove()}>{words.removeAvatar}</button>}</div>
 </section>;
}
