import type { Messages } from '../i18n/messages';

export function getFormError(
  form: HTMLFormElement,
  words: Messages,
): string | null {
  if (form.checkValidity()) {
    return null;
  }

  const field = form.querySelector<
    HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
  >('input:invalid, textarea:invalid, select:invalid');

  if (!field) {
    return words.invalidForm;
  }

  field.focus();

  if (field.validity.valueMissing) {
    return words.requiredField;
  }

  if (
    field.validity.typeMismatch &&
    field instanceof HTMLInputElement &&
    field.type === 'email'
  ) {
    return words.invalidEmail;
  }

  if (field.validity.tooShort) {
    return words.tooShort;
  }

  if (field.validity.tooLong) {
    return words.tooLong;
  }

  return words.invalidForm;
}