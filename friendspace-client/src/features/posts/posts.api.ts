import { request } from '../../shared/api/http-client';
import type { Post, PostInput } from './posts.types';
export const postsApi = {
  getPosts: (page: number, signal?: AbortSignal) => request<Post[]>(`/posts?pageNumber=${page}&pageSize=10`, {
    signal
  }),
  getPost: (id: string, signal?: AbortSignal) => request<Post>(`/posts/${id}`, {
    signal
  }),
  createPost: (input: PostInput) => request<Post>('/posts', {
    method: 'POST',
    body: JSON.stringify(input)
  }),
  updatePost: (id: string, input: PostInput) => request<Post>(`/posts/${id}/content`, {
    method: 'PUT',
    body: JSON.stringify(input)
  }),
  deletePost: (id: string) => request<void>(`/posts/${id}`, {
    method: 'DELETE'
  })
};
