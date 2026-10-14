import { request } from '../../shared/api/http-client';
export interface Likes {
  postId: string;
  likeCount: number;
  isLikedByMe: boolean;
}
export const likesApi = {
  get: (id: string, signal?: AbortSignal) => request<Likes>(`/posts/${id}/likes`, {
    signal
  }),
  set: (id: string, liked: boolean) => request<void>(`/posts/${id}/likes/me`, {
    method: liked ? 'PUT' : 'DELETE'
  })
};
