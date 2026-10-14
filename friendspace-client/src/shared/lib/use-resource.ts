import { useEffect, useReducer, useState } from 'react';
import { useErrors } from './use-errors';
export function useResource<T>(loader: (signal: AbortSignal) => Promise<T>) {
  const [state, setState] = useState<{
    data: T | null;
    loading: boolean;
    failed: boolean;
  }>({
    data: null,
    loading: true,
    failed: false
  });
  const [version, reload] = useReducer((n: number) => n + 1, 0);
  const report = useErrors();
  useEffect(() => {
    const controller = new AbortController();
    let active = true;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setState({
      data: null,
      loading: true,
      failed: false
    });
    loader(controller.signal).then(data => {
      if (active) setState({
        data,
        loading: false,
        failed: false
      });
    }).catch(error => {
      if (active) {
        setState({
          data: null,
          loading: false,
          failed: true
        });
        report(error);
      }
    });
    return () => {
      active = false;
      controller.abort();
    };
  }, [loader, version, report]);
  return {
    ...state,
    reload
  };
}
