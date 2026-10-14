import { ApiError, getCsrfToken, parseResponse, SESSION_INVALID, request } from '../../shared/api/http-client';
export interface Media {
  id: string;
  url: string;
}
export const imageTypes = ['image/jpeg', 'image/png', 'image/webp'];
export const maxImageBytes = 5 * 1024 * 1024;
export function uploadImage(file: File, onProgress: (value: number) => void, signal?: AbortSignal, avatar = false): Promise<Media> {
  return new Promise((resolve, reject) => {
    if (!imageTypes.includes(file.type)) {
      reject(new ApiError(400, 'Unsupported image', 'image_type'));
      return;
    }
    if (file.size > maxImageBytes) {
      reject(new ApiError(400, 'Image too large', 'image_too_large'));
      return;
    }
    const token = getCsrfToken();
    if (!token) {
      const error = new ApiError(400, 'Session changed', 'invalid_csrf');
      window.dispatchEvent(new CustomEvent(SESSION_INVALID, {
        detail: error
      }));
      reject(error);
      return;
    }
    const xhr = new XMLHttpRequest();
    const path = avatar ? '/profiles/me/avatar' : '/media';
    const abort = () => xhr.abort();
    if (signal?.aborted) {
      reject(new DOMException('Aborted', 'AbortError'));
      return;
    }
    xhr.open(avatar ? 'PUT' : 'POST', `/api${path}`);
    xhr.withCredentials = true;
    xhr.timeout = 120000;
    xhr.setRequestHeader('X-CSRF-TOKEN', token);
    xhr.setRequestHeader('Accept', 'application/json');
    xhr.upload.onprogress = event => {
      if (event.lengthComputable) onProgress(Math.round(event.loaded / event.total * 100));
    };
    xhr.onload = () => {
      try {
        resolve(parseResponse<Media>(xhr.status, xhr.responseText, path));
      } catch (error) {
        reject(error);
      }
    };
    xhr.onerror = () => reject(new TypeError('Network error'));
    xhr.ontimeout = () => reject(new TypeError('Upload timed out'));
    xhr.onabort = () => reject(new DOMException('Aborted', 'AbortError'));
    xhr.onloadend = () => signal?.removeEventListener('abort', abort);
    signal?.addEventListener('abort', abort, {
      once: true
    });
    const body = new FormData();
    body.append('file', file);
    xhr.send(body);
  });
}
export const removeStagedImage = (id: string) => request<void>(`/media/${id}`, {
  method: 'DELETE'
});
