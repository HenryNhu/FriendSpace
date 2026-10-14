import { useId } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import { imageTypes, maxImageBytes, removeStagedImage } from './media.api';
import { useLanguage } from '../../shared/i18n/language-context';
import { useToast } from '../../shared/ui/toast/toast-context';
import { useErrors } from '../../shared/lib/use-errors';
export interface DraftImage {
  key: string;
  id?: string;
  preview: string;
  file?: File;
  fresh: boolean;
  progress: number;
  failed: boolean;
}
export function ImagePicker({
  items,
  setItems,
  busy,
  trackUrl
}: {
  items: DraftImage[];
  setItems: Dispatch<SetStateAction<DraftImage[]>>;
  busy: boolean;
  trackUrl: (url: string) => void;
}) {
  const {
    words
  } = useLanguage();
  const {
    notify
  } = useToast();
  const report = useErrors();
  const id = useId();
  function select(files: FileList | null) {
    if (!files) return;
    const list = Array.from(files);
    if (items.length + list.length > 4) {
      notify(words.tooManyImages, 'error');
      return;
    }
    if (list.some(file => !imageTypes.includes(file.type))) {
      notify(words.invalidImage, 'error');
      return;
    }
    if (list.some(file => file.size > maxImageBytes)) {
      notify(words.imageTooLarge, 'error');
      return;
    }
    const additions = list.map(file => {
      const preview = URL.createObjectURL(file);
      trackUrl(preview);
      return {
        key: crypto.randomUUID(),
        preview,
        file,
        fresh: true,
        progress: 0,
        failed: false
      };
    });
    setItems(current => [...current, ...additions]);
  }
  function remove(item: DraftImage) {
    setItems(current => current.filter(value => value.key !== item.key));
    if (item.file) URL.revokeObjectURL(item.preview);
    if (item.fresh && item.id) void removeStagedImage(item.id).catch(report);
  }
  return <div className="image-picker">
  <label htmlFor={id}>{words.addPhotos}</label><input id={id} type="file" accept="image/jpeg,image/png,image/webp" multiple disabled={busy || items.length >= 4} onChange={event => {
      select(event.target.files);
      event.target.value = '';
    }} />
  <p className="muted">{words.imageHint}</p>
  <div className="image-previews">{items.map(item => <figure key={item.key}>
   <img src={item.preview} alt={words.postImage} />
   <button type="button" disabled={busy} onClick={() => remove(item)} aria-label={words.removePhoto}>×</button>
   {busy && !item.id && <><progress aria-label={words.uploadProgress} value={item.progress} max={100} /><small>{item.progress === 100 ? words.processingImage : `${item.progress}%`}</small></>}
   {item.failed && <small role="status">{words.imageFailed}</small>}
  </figure>)}</div>
 </div>;
}
