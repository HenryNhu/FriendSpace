import { request } from '../../shared/api/http-client';
import type { FeedApi, FeedPage } from './feed.types';

export const feedApi: FeedApi = {
  getPage(cursor, signal) {
    const query = new URLSearchParams({ pageSize: '10' });
    if (cursor) query.set('cursor', cursor);
    return request<FeedPage>(`/feed?${query}`, { signal });
  },
  getUpdates(after, signal) {
    const query = new URLSearchParams();
    if (after) query.set('after', after);
    return request<{ hasNewPosts: boolean }>(`/feed/updates?${query}`, { signal });
  },
};
