import type { Messages } from '../../../shared/i18n/messages';
import { useToast } from '../../../shared/ui/toast/toast-context';
import { getFormError } from '../../../shared/lib/get-form-error';

interface PostComposerProps {
  words: Messages;
  displayName: string;
  content: string;
  busy: boolean;
  onContentChange: (value: string) => void;
  onSubmit: () => void;
}

export function PostComposer(props: PostComposerProps) {
  const { words, busy } = props;
  const { notify } = useToast();

  return (
    <section className="card">
      <form
        noValidate
        onSubmit={(event) => {
          event.preventDefault();

          if (busy) {
            return;
          }

          const form = event.currentTarget;
          const validationMessage = getFormError(form, words);

          if (validationMessage) {
            notify(validationMessage, 'error');
            return;
          }

          if (!props.content.trim()) {
            notify(words.requiredField, 'error');

            form
              .querySelector<HTMLTextAreaElement>(
                'textarea[name="content"]',
              )
              ?.focus();

            return;
          }

          props.onSubmit();
        }}
      >
        <fieldset disabled={busy}>
          <p>
            <strong>{props.displayName}</strong>
          </p>

          <textarea
            name="content"
            rows={3}
            required
            aria-label={words.placeholder}
            placeholder={words.placeholder}
            value={props.content}
            onChange={(event) =>
              props.onContentChange(event.target.value)
            }
          />

          <div className="form-actions">
            <button
              type="submit"
              className="primary"
              disabled={busy}
            >
              {words.publish}
            </button>
          </div>
        </fieldset>
      </form>
    </section>
  );
}