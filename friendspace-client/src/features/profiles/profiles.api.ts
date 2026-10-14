import { request } from '../../shared/api/http-client';
import type { Profile } from './profiles.types';
import type { Post } from '../posts/posts.types';
export const profilesApi = {
  getProfile: () => request<Profile>('/profiles/me'),
  getPublicProfile: (id: string, signal?: AbortSignal) => request<Profile>(`/profiles/${id}`, {
    signal
  }),
  getPosts: (id: string, page: number, signal?: AbortSignal) => request<Post[]>(`/profiles/${id}/posts?pageNumber=${page}&pageSize=10`, {
    signal
  }),
  search: (query: string, page: number, signal?: AbortSignal) => request<Profile[]>(`/profiles?query=${encodeURIComponent(query)}&pageNumber=${page}&pageSize=10`, {
    signal
  }),
  saveProfile: (displayName: string, bio: string) => request<Profile>('/profiles/me', {
    method: 'PUT',
    body: JSON.stringify({
      displayName,
      bio
    })
  }),
  removeAvatar: () => request<void>('/profiles/me/avatar', {
    method: 'DELETE'
  })
};
