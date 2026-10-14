import { useCallback, useState } from 'react';
import { Link, useSearchParams } from 'react-router';
import { profilesApi } from '../../features/profiles/profiles.api';
import { useResource } from '../../shared/lib/use-resource';
import { useLanguage } from '../../shared/i18n/language-context';
import { Avatar } from '../../shared/ui/Avatar';
import { Pagination } from '../../shared/ui/Pagination';
export function PeoplePage() {
  const {
    words
  } = useLanguage();
  const [params, setParams] = useSearchParams();
  const query = params.get('q') ?? '';
  const raw = Number(params.get('page') ?? 1);
  const page = Number.isSafeInteger(raw) && raw > 0 ? raw : 1;
  const [draft, setDraft] = useState(query);
  const loader = useCallback((signal: AbortSignal) => profilesApi.search(query, page, signal), [query, page]);
  const resource = useResource(loader);
  return <><h1>{words.people}</h1><form className="card search-form" onSubmit={event => {
      event.preventDefault();
      setParams({
        q: draft.trim(),
        page: '1'
      });
    }}><label>{words.searchName}<input type="search" maxLength={80} value={draft} onChange={event => setDraft(event.target.value)} /></label><button className="primary">{words.search}</button><p className="muted">{words.searchHint}</p></form>
 {resource.loading && <p role="status">{words.loading}</p>}{resource.failed && <button onClick={resource.reload}>{words.retry}</button>}{resource.data?.length === 0 && <p className="card">{words.noPeople}</p>}
 <div className="people-grid">{resource.data?.map(person => <article className="card" key={person.userId}><div className="person"><Avatar name={person.displayName} url={person.avatarUrl} /><Link to={`/profiles/${person.userId}`}>{person.displayName}</Link></div><p className="content">{person.bio}</p><Link to={`/profiles/${person.userId}`}>{words.viewProfile}</Link></article>)}</div>
 {resource.data && <Pagination page={page} count={resource.data.length} onChange={value => setParams({
      q: query,
      page: String(value)
    })} />}
 </>;
}
