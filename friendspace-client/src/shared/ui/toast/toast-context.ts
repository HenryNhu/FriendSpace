import { createContext, useContext } from 'react';

export type ToastKind = 'success' | 'error' | 'info';

interface ToastContextValue {
  notify: (message: string, kind?: ToastKind) => void;
}

export const ToastContext =
  createContext<ToastContextValue | null>(null);

export function useToast(): ToastContextValue {
  const context = useContext(ToastContext);

  if (!context) {
    throw new Error(
      'useToast phải được sử dụng bên trong ToastProvider.',
    );
  }

  return context;
}