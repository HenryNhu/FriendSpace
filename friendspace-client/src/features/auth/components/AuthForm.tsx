import type { Messages } from '../../../shared/i18n/messages';
import { useToast } from '../../../shared/ui/toast/toast-context';
import { getFormError } from '../../../shared/lib/get-form-error';

interface AuthFormProps {
  words: Messages;
  mode: 'login' | 'register';
  email: string;
  password: string;
  busy: boolean;
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onModeChange: (value: 'login' | 'register') => void;
  onSubmit: () => void;
}

export function AuthForm(props: AuthFormProps) {
  const { words, mode, busy } = props;
  const { notify } = useToast();

  return (
    <section className="card">
      <h2>{words[mode]}</h2>

      <form
        noValidate
        onSubmit={(event) => {
          event.preventDefault();

          if (busy) {
            return;
          }

          const validationMessage = getFormError(
            event.currentTarget,
            words,
          );

          if (validationMessage) {
            notify(validationMessage, 'error');
            return;
          }

          props.onSubmit();
        }}
      >
        <fieldset disabled={busy}>
          <label>
            {words.email}

            <input
              type="email"
              name="email"
              autoComplete="username"
              required
              value={props.email}
              onChange={(event) =>
                props.onEmailChange(event.target.value)
              }
            />
          </label>

          <label>
            {words.password}

            <input
              type="password"
              name="password"
              autoComplete={
                mode === 'register'
                  ? 'new-password'
                  : 'current-password'
              }
              required
              minLength={mode === 'register' ? 8 : undefined}
              value={props.password}
              onChange={(event) =>
                props.onPasswordChange(event.target.value)
              }
            />
          </label>

          {mode === 'register' && (
            <p className="muted">{words.passwordHint}</p>
          )}

          <div className="form-actions">
            <button
              type="submit"
              className="primary"
              disabled={busy}
            >
              {words[mode]}
            </button>

            <button
              type="button"
              disabled={busy}
              onClick={() =>
                props.onModeChange(
                  mode === 'login' ? 'register' : 'login',
                )
              }
            >
              {mode === 'login' ? words.register : words.login}
            </button>
          </div>
        </fieldset>
      </form>
    </section>
  );
}