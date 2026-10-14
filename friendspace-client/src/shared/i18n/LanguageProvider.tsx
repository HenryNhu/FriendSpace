import { useEffect, useState, type ReactNode } from 'react';
import { LanguageContext } from './language-context';
import { messages, type Language } from './messages';
export function LanguageProvider({
  children
}: {
  children: ReactNode;
}) {
  const [language, setLanguage] = useState<Language>(() => {
    try {
      return localStorage.getItem('friendspace.language') === 'en' ? 'en' : 'vi';
    } catch {
      return 'vi';
    }
  });
  useEffect(() => {
    document.documentElement.lang = language;
    document.title = 'FriendSpace';
    try {
      localStorage.setItem('friendspace.language', language);
    } catch {/* Language still works without storage. */}
  }, [language]);
  return <LanguageContext value={{
    language,
    words: messages[language],
    setLanguage
  }}>{children}</LanguageContext>;
}
