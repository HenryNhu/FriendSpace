import { useCallback } from 'react';
import { ApiError, sessionError } from '../api/http-client';
import { useLanguage } from '../i18n/language-context';
import { useToast } from '../ui/toast/toast-context';
export function useErrors() {
  const {
    words
  } = useLanguage();
  const {
    notify
  } = useToast();
  return useCallback((error: unknown) => {
    if (error instanceof DOMException && error.name === 'AbortError') return;
    if (sessionError(error)) return;
    if (!(error instanceof ApiError)) {
      notify(words.connectionError, 'error');
      return;
    }
    const codeMessages: Record<string, string> = {
      not_found: words.notFound,
      forbidden: words.forbidden,
      invalid_media: words.invalidImage,
      image_type: words.invalidImage,
      image_too_large: words.imageTooLarge,
      media_unavailable: words.mediaUnavailable,
      friendship_exists: words.friendshipExists,
      validation_error: words.invalidForm,
      profile_required: words.completeProfile
    };
    notify(codeMessages[error.code ?? ''] ?? (error.status === 404 ? words.notFound : error.status === 403 ? words.forbidden : error.status === 413 ? words.imageTooLarge : error.message), 'error');
  }, [notify, words]);
}
