import type { Post } from '../posts/posts.types';
import type { FeedApi } from './feed.types';

interface FeedState {
  items: Post[];
  nextCursor: string | null;
  topCursor: string | null;
  hasMore: boolean;
  hasFriends: boolean;
  initialized: boolean;
  loading: boolean;
  loadingMore: boolean;
  refreshFailed: boolean;
  loadMoreFailed: boolean;
  pollingFailed: boolean;
  hasNewPosts: boolean;
}

// One instance per mounted feed; never shared between user sessions.
export class FeedStore {
  private state: FeedState = {
    items: [], nextCursor: null, topCursor: null, hasMore: false,
    hasFriends: false, initialized: false, loading: true, loadingMore: false,
    refreshFailed: false, loadMoreFailed: false, pollingFailed: false, hasNewPosts: false,
  };
  private listeners = new Set<() => void>();
  private generation = 0;
  private active = false;
  private pageRequest?: AbortController;
  private moreRequest?: AbortController;
  private pollRequest?: AbortController;
  private reportError: (error: unknown) => void = () => {};
  private api: FeedApi;

  constructor(api: FeedApi) { this.api = api; }

  getSnapshot = () => this.state;
  subscribe = (listener: () => void) => {
    this.listeners.add(listener);
    return () => { this.listeners.delete(listener); };
  };
  setErrorHandler(handler: (error: unknown) => void) { this.reportError = handler; }

  private update(patch: Partial<FeedState>) {
    this.state = { ...this.state, ...patch };
    this.listeners.forEach(listener => listener());
  }
  private current(generation: number, request: AbortController) {
    return this.active && generation === this.generation && !request.signal.aborted;
  }

  start = () => { this.active = true; void this.refresh(); };
  stop = () => {
    this.active = false;
    this.generation++;
    this.pageRequest?.abort();
    this.moreRequest?.abort();
    this.pollRequest?.abort();
  };

  refresh = async () => {
    if (!this.active) return;
    const generation = ++this.generation;
    this.pageRequest?.abort();
    this.moreRequest?.abort();
    this.pollRequest?.abort();
    this.pollRequest = undefined;
    const request = new AbortController();
    this.pageRequest = request;
    this.update({ loading: true, loadingMore: false, refreshFailed: false, loadMoreFailed: false });
    try {
      const page = await this.api.getPage(null, request.signal);
      if (!this.current(generation, request)) return;
      this.update({ ...page, initialized: true, loading: false, hasNewPosts: false, pollingFailed: false });
    } catch (error) {
      if (!this.current(generation, request)) return;
      this.update({ loading: false, refreshFailed: true });
      this.reportError(error);
    }
  };

  loadMore = async () => {
    if (!this.active || this.state.loading || this.state.loadingMore || !this.state.nextCursor) return;
    const generation = this.generation;
    const request = new AbortController();
    this.moreRequest = request;
    this.update({ loadingMore: true, loadMoreFailed: false });
    try {
      const page = await this.api.getPage(this.state.nextCursor, request.signal);
      if (!this.current(generation, request)) return;
      const seen = new Set(this.state.items.map(post => post.id));
      const additional = page.items.filter(post => {
        if (seen.has(post.id)) return false;
        seen.add(post.id);
        return true;
      });
      // Preserve the first-page anchor for new-post detection.
      this.update({ items: [...this.state.items, ...additional], nextCursor: page.nextCursor,
        hasMore: page.hasMore, hasFriends: page.hasFriends, loadingMore: false });
    } catch (error) {
      if (!this.current(generation, request)) return;
      this.update({ loadingMore: false, loadMoreFailed: true });
      this.reportError(error);
    }
  };

  poll = async () => {
    if (!this.active || !this.state.initialized || this.state.loading ||
        this.state.hasNewPosts || this.state.pollingFailed || this.pollRequest) return;
    const generation = this.generation;
    const request = new AbortController();
    this.pollRequest = request;
    try {
      const result = await this.api.getUpdates(this.state.topCursor, request.signal);
      if (this.current(generation, request)) this.update({ hasNewPosts: result.hasNewPosts });
    } catch (error) {
      if (!this.current(generation, request)) return;
      // Report once. A successful manual refresh resumes background checks.
      this.update({ pollingFailed: true });
      this.reportError(error);
    } finally {
      if (this.pollRequest === request) this.pollRequest = undefined;
    }
  };
}
