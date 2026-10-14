import { request } from '../../shared/api/http-client';
export interface Comment {
  id: string;
  postId: string;
  authorId: string;
  authorDisplayName: string | null;
  authorAvatarUrl: string | null;
  content: string;
  createdAt: string;
  updatedAt: string | null;
}
export const commentsApi = {
  list: (postId: string, page: number, signal?: AbortSignal) => request<Comment[]>(`/posts/${postId}/comments?pageNumber=${page}&pageSize=10`, {
    signal
  }),
  create: (postId: string, content: string) => request<Comment>(`/posts/${postId}/comments`, {
    method: 'POST',
    body: JSON.stringify({
      content
    })
  }),
  update: (postId: string, id: string, content: string) => request<Comment>(`/posts/${postId}/comments/${id}/content`, {
    method: 'PUT',
    body: JSON.stringify({
      content
    })
  }),
  remove: (postId: string, id: string) => request<void>(`/posts/${postId}/comments/${id}`, {
    method: 'DELETE'
  })
};
