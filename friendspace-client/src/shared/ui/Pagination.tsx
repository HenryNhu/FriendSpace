import { useLanguage } from '../i18n/language-context';
export function Pagination({
  page,
  count,
  size = 10,
  onChange
}: {
  page: number;
  count: number;
  size?: number;
  onChange: (page: number) => void;
}) {
  const {
    words
  } = useLanguage();
  return <nav className="pagination" aria-label={words.pagination}><button disabled={page === 1} onClick={() => onChange(page - 1)}>{words.previous}</button><span>{words.page} {page}</span><button disabled={count < size} onClick={() => onChange(page + 1)}>{words.next}</button></nav>;
}
