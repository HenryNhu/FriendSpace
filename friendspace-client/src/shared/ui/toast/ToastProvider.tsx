import {
  useCallback,
  useMemo,
  useState,
  type ReactNode,
} from 'react';

import { createPortal } from 'react-dom';

import {
  ToastContext,
  type ToastKind,
} from './toast-context';

import styles from './Toast.module.css';

interface ToastItem {
  id: string;
  message: string;
  kind: ToastKind;
  closeLabel: string;
}

interface ToastProviderProps {
  children: ReactNode;
}

export function ToastProvider({
  children,
}: ToastProviderProps) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const dismiss = useCallback((id: string) => {
    setToasts((current) =>
      current.filter((toast) => toast.id !== id),
    );
  }, []);

  const notify = useCallback(
    (message: string, kind: ToastKind = 'info') => {
      const toast: ToastItem = {
        id: crypto.randomUUID(),
        message,
        kind,
        closeLabel:
          document.documentElement.lang === 'en'
            ? 'Close notification'
            : 'Đóng thông báo',
      };

      setToasts((current) => [...current, toast]);
    },
    [],
  );

  const contextValue = useMemo(
    () => ({ notify }),
    [notify],
  );

  return (
    <ToastContext.Provider value={contextValue}>
      {children}

      {createPortal(
        <div className={styles.viewport}>
          {toasts.map((toast) => (
            <article
              key={toast.id}
              className={`${styles.toast} ${styles[toast.kind]}`}
            >
              <p
                className={styles.message}
                role={toast.kind === 'error' ? 'alert' : 'status'}
                aria-atomic="true"
              >
                {toast.message}
              </p>

              <button
                  type="button"
                  className={styles.close}
                  aria-label={toast.closeLabel}
                  title={toast.closeLabel}
                  onClick={() => dismiss(toast.id)}
                >
                  <svg
                    viewBox="0 0 40 40"
                    className={styles.timer}
                    aria-hidden="true"
                    focusable="false"
                  >
                    <circle
                      className={styles.track}
                      cx="20"
                      cy="20"
                      r="17"
                    />

                    <circle
                      className={styles.progress}
                      cx="20"
                      cy="20"
                      r="17"
                      pathLength="100"
                      transform="rotate(-90 20 20)"
                      onAnimationEnd={() => dismiss(toast.id)}
                    />

                    <path
                      d="M16 16 L24 24 M24 16 L16 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.8"
                      strokeLinecap="round"
                    />
                  </svg>
                </button>
            </article>
          ))}
        </div>,
        document.body,
      )}
    </ToastContext.Provider>
  );
}