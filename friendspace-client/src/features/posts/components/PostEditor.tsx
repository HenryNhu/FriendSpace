import { useEffect, useRef, useState } from 'react';
import type { Post, PostInput, Visibility } from '../posts.types';
import { ImagePicker, type DraftImage } from '../../media/ImagePicker';
import { uploadImage } from '../../media/media.api';
import { useLanguage } from '../../../shared/i18n/language-context';
import { useToast } from '../../../shared/ui/toast/toast-context';
import { useErrors } from '../../../shared/lib/use-errors';
export function PostEditor({
  initial,
  onSave,
  onCancel
}: {
  initial?: Post;
  onSave: (input: PostInput) => Promise<void>;
  onCancel?: () => void;
}) {
  const {
    words
  } = useLanguage();
  const {
    notify
  } = useToast();
  const report = useErrors();
  const [content, setContent] = useState(initial?.content ?? '');
  const [visibility, setVisibility] = useState<Visibility>(initial?.visibility ?? 0);
  const [items, setItems] = useState<DraftImage[]>(() => (initial?.images ?? []).map(image => ({
    key: image.id,
    id: image.id,
    preview: image.url,
    fresh: false,
    progress: 100,
    failed: false
  })));
  const [busy, setBusy] = useState(false);
  const lock = useRef(false);
  const urls = useRef(new Set<string>());
  const controller = useRef<AbortController | null>(null);
  useEffect(() => {
    const values = urls.current;
    return () => {
      controller.current?.abort();
      values.forEach(URL.revokeObjectURL);
    };
  }, []);
  async function submit() {
    if (lock.current) return;
    if (!content.trim()) {
      notify(words.requiredField, 'error');
      return;
    }
    if (content.trim().length > 10000) {
      notify(words.tooLong, 'error');
      return;
    }
    lock.current = true;
    setBusy(true);
    controller.current = new AbortController();
    try {
      const mediaIds: string[] = [];
      for (const item of items) {
        if (item.id) {
          mediaIds.push(item.id);
          continue;
        }
        if (!item.file) continue;
        setItems(current => current.map(value => value.key === item.key ? {
          ...value,
          failed: false
        } : value));
        try {
          const media = await uploadImage(item.file, progress => setItems(current => current.map(value => value.key === item.key ? {
            ...value,
            progress
          } : value)), controller.current.signal);
          mediaIds.push(media.id);
          setItems(current => current.map(value => value.key === item.key ? {
            ...value,
            id: media.id,
            progress: 100
          } : value));
        } catch (error) {
          setItems(current => current.map(value => value.key === item.key ? {
            ...value,
            failed: true
          } : value));
          throw error;
        }
      }
      if (controller.current.signal.aborted) return;
      await onSave({
        content: content.trim(),
        visibility,
        mediaIds
      });
      setContent('');
      setItems([]);
      urls.current.forEach(URL.revokeObjectURL);
      urls.current.clear();
    } catch (error) {
      report(error);
    } finally {
      lock.current = false;
      setBusy(false);
    }
  }
  return <form noValidate onSubmit={event => {
    event.preventDefault();
    void submit();
  }}>
  <fieldset disabled={busy}>
   <label>{words.placeholder}<textarea rows={4} name="content" required maxLength={10000} value={content} onChange={event => setContent(event.target.value)} /></label>
   <label>{words.visibility}<select value={visibility} onChange={event => setVisibility(Number(event.target.value) as Visibility)}>
    <option value={0}>{words.public}</option><option value={1}>{words.friendsOnly}</option><option value={2}>{words.onlyMe}</option>
   </select></label>
   <ImagePicker items={items} setItems={setItems} busy={busy} trackUrl={url => urls.current.add(url)} />
   <div className="form-actions"><button type="submit" className="primary" disabled={busy}>{busy ? words.loading : initial ? words.saveChanges : words.publish}</button>{onCancel && <button type="button" disabled={busy} onClick={onCancel}>{words.cancel}</button>}</div>
  </fieldset>
 </form>;
}
