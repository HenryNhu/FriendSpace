export class ApiError extends Error {
  status: number;
  code?: string;
  constructor(status: number, message: string, code?: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
  }
}
interface ApiProblem {
  code?: string;
  message?: string;
  detail?: string;
  title?: string;
  errors?: Record<string, string[]>;
}
let csrfToken: string | null = null;
export const SESSION_INVALID = 'friendspace:session-invalid';
export function setCsrfToken(value: string | null) {
  csrfToken = value;
}
export function getCsrfToken() {
  return csrfToken;
}
export function sessionError(error: unknown): boolean {
  return error instanceof ApiError && (error.status === 401 || error.code === 'invalid_csrf');
}
export function parseResponse<T>(status: number, text: string, path: string): T {
  let value: unknown;
  try {
    value = text ? JSON.parse(text) : undefined;
  } catch {
    value = undefined;
  }
  if (status < 200 || status >= 300) {
    const problem = value as ApiProblem | undefined;
    const message = problem?.errors ? Object.values(problem.errors).flat().join(' ') : undefined;
    const error = new ApiError(status, message || problem?.message || problem?.detail || problem?.title || `HTTP ${status}`, problem?.code);
    if (sessionError(error) && !path.startsWith('/auth/login') && !path.startsWith('/auth/register') && path !== '/auth/session') {
      window.dispatchEvent(new CustomEvent(SESSION_INVALID, {
        detail: error
      }));
    }
    throw error;
  }
  if (text && value === undefined) throw new ApiError(status, 'Invalid JSON response', 'invalid_response');
  return value as T;
}
export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const method = (options.method ?? 'GET').toUpperCase();
  const headers = new Headers(options.headers);
  headers.set('Accept', 'application/json');
  if (options.body != null && !(options.body instanceof FormData)) headers.set('Content-Type', 'application/json');
  if (!['GET', 'HEAD', 'OPTIONS'].includes(method)) {
    if (!csrfToken) {
      const error = new ApiError(400, 'Session changed', 'invalid_csrf');
      window.dispatchEvent(new CustomEvent(SESSION_INVALID, {
        detail: error
      }));
      throw error;
    }
    headers.set('X-CSRF-TOKEN', csrfToken);
  }
  const response = await fetch(`/api${path}`, {
    ...options,
    method,
    headers,
    credentials: 'include',
    cache: 'no-store'
  });
  return parseResponse<T>(response.status, await response.text(), path);
}
