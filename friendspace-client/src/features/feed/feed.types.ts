import type { Post } from '../posts/posts.types';

export interface FeedPage {
  items: Post[];
  nextCursor: string | null;
  topCursor: string | null;
  hasMore: boolean;
  hasFriends: boolean;
}

export interface FeedApi {
  getPage(cursor: string | null, signal: AbortSignal): Promise<FeedPage>;
  getUpdates(after: string | null, signal: AbortSignal): Promise<{ hasNewPosts: boolean }>;
}
