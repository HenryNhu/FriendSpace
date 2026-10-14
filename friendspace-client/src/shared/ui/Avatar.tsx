export function Avatar({
  name,
  url,
  large = false
}: {
  name: string;
  url?: string | null;
  large?: boolean;
}) {
  return <span className={`avatar ${large ? 'avatar-large' : ''}`} aria-hidden="true">
    {url ? <img src={url} alt="" loading="lazy" onError={event => {
      event.currentTarget.style.display = 'none';
    }} /> : (Array.from(name.trim())[0] ?? 'F').toUpperCase()}
  </span>;
}
