import { createContext, useContext } from 'react';
import type { Language, Messages } from './messages';
export const LanguageContext = createContext<{
  language: Language;
  words: Messages;
  setLanguage: (value: Language) => void;
} | null>(null);
export function useLanguage() {
  const context = useContext(LanguageContext);
  if (!context) throw new Error('LanguageProvider is missing');
  return context;
}
