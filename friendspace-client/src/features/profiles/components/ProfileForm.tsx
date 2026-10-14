import type { Messages } from '../../../shared/i18n/messages';
import { useToast } from '../../../shared/ui/toast/toast-context';
import { getFormError } from '../../../shared/lib/get-form-error';

interface ProfileFormProps {
  words: Messages;
  email: string | null;
  displayName: string;
  bio: string;
  busy: boolean;
  onDisplayNameChange: (value: string) => void;
  onBioChange: (value: string) => void;
  onSubmit: () => void;
}

export function ProfileForm(props: ProfileFormProps) {
  const { words, busy } = props;
  const { notify } = useToast();

  return (
    <section className="card">
      <h2>{words.profile}</h2>

      <p className="muted">{props.email}</p>

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

          // required/minLength chưa loại được tên toàn khoảng trắng.
          const normalizedName = props.displayName.trim();

          if (normalizedName.length < 2) {
            notify(
              normalizedName.length === 0
                ? words.requiredField
                : words.tooShort,
              'error',
            );

            form
              .querySelector<HTMLInputElement>(
                'input[name="displayName"]',
              )
              ?.focus();

            return;
          }

          props.onSubmit();
        }}
      >
        <fieldset disabled={busy}>
          <label>
            {words.displayName}

            <input
              name="displayName"
              autoComplete="nickname"
              required
              minLength={2}
              maxLength={80}
              value={props.displayName}
              onChange={(event) =>
                props.onDisplayNameChange(event.target.value)
              }
            />
          </label>

          <label>
            {words.bio}

            <textarea
              name="bio"
              rows={4}
              maxLength={500}
              value={props.bio}
              onChange={(event) =>
                props.onBioChange(event.target.value)
              }
            />
          </label>

          <div className="form-actions">
            <button
              type="submit"
              className="primary"
              disabled={busy}
            >
              {words.save}
            </button>
          </div>
        </fieldset>
      </form>
    </section>
  );
}