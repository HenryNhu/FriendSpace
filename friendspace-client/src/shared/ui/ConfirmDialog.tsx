import { useEffect, useId, useRef } from 'react';
import { createPortal } from 'react-dom';
import { useLanguage } from '../i18n/language-context';
export function ConfirmDialog({
  title,
  message,
  busy,
  onCancel,
  onConfirm
}: {
  title: string;
  message: string;
  busy: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}) {
  const ref = useRef<HTMLDialogElement>(null);
  const id = useId();
  const {
    words
  } = useLanguage();
  useEffect(() => {
    const dialog = ref.current;
    dialog?.showModal();
    return () => dialog?.close();
  }, []);
  return createPortal(<dialog ref={ref} className="confirm-dialog" aria-labelledby={`${id}-title`} aria-describedby={`${id}-text`} onCancel={event => {
    event.preventDefault();
    if (!busy) onCancel();
  }}>
    <h2 id={`${id}-title`}>{title}</h2><p id={`${id}-text`}>{message}</p>
    <div className="form-actions"><button type="button" autoFocus disabled={busy} onClick={onCancel}>{words.cancel}</button><button type="button" className="danger" disabled={busy} onClick={onConfirm}>{busy ? words.loading : words.confirm}</button></div>
  </dialog>, document.body);
}
