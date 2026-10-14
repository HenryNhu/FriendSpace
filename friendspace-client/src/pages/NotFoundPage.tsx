import { Link } from 'react-router';
import { useLanguage } from '../shared/i18n/language-context';
export function NotFoundPage() {
  const {
    words
  } = useLanguage();
  return <section className="card"><h1>404</h1><p>{words.notFound}</p><Link to="/">{words.backToFeed}</Link></section>;
}
