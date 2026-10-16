import { useEffect, useState, useSyncExternalStore } from 'react';
import { useErrors } from '../../shared/lib/use-errors';
import { feedApi } from './feed.api';
import { FeedStore } from './feed-store';

export function useFeed() {
  const reportError = useErrors();
  const [store] = useState(() => new FeedStore(feedApi));
  const state = useSyncExternalStore(store.subscribe, store.getSnapshot);

  useEffect(() => { store.setErrorHandler(reportError); }, [store, reportError]);
  useEffect(() => {
    store.start();
    const check = () => {
      if (document.visibilityState === 'visible') void store.poll();
    };
    const timer = window.setInterval(check, 30_000);
    document.addEventListener('visibilitychange', check);
    return () => {
      window.clearInterval(timer);
      document.removeEventListener('visibilitychange', check);
      store.stop();
    };
  }, [store]);

  return { ...state, refresh: store.refresh, loadMore: store.loadMore };
}
