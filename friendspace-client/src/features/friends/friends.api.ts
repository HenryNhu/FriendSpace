import { request } from '../../shared/api/http-client';
export type FriendshipState = 'none' | 'self' | 'incoming' | 'outgoing' | 'accepted';
export interface Relationship {
  id: string | null;
  state: FriendshipState;
}
export interface Friend {
  id: string;
  otherUserId: string;
  displayName: string;
  avatarUrl: string | null;
  state: Exclude<FriendshipState, 'none' | 'self'>;
}
export const friendsApi = {
  list: (signal?: AbortSignal) => request<Friend[]>('/friendships', {
    signal
  }),
  withUser: (id: string, signal?: AbortSignal) => request<Relationship>(`/friendships/with/${id}`, {
    signal
  }),
  invite: (userId: string) => request<void>('/friendships', {
    method: 'POST',
    body: JSON.stringify({
      userId
    })
  }),
  accept: (id: string) => request<void>(`/friendships/${id}/accept`, {
    method: 'POST'
  }),
  remove: (id: string) => request<void>(`/friendships/${id}`, {
    method: 'DELETE'
  })
};
