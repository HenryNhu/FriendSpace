import { request } from '../../shared/api/http-client';
import type { Profile } from './profiles.types';

export const profilesApi = {
  getProfile(): Promise<Profile> {
    return request<Profile>('/profiles/me');
  },

  saveProfile(
    displayName: string,
    bio: string,
  ): Promise<Profile> {
    return request<Profile>('/profiles/me', {
      method: 'PUT',
      body: JSON.stringify({ displayName, bio }),
    });
  },
};