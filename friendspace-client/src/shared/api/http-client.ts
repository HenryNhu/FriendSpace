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

export function setCsrfToken(token: string | null): void {
  csrfToken = token;
}

export async function request<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const method = (options.method ?? 'GET').toUpperCase();
  const headers = new Headers(options.headers);

  headers.set('Accept', 'application/json');

  if (options.body !== undefined && options.body !== null) {
    headers.set('Content-Type', 'application/json');
  }

  const isReadRequest = ['GET', 'HEAD', 'OPTIONS'].includes(method);

  if (!isReadRequest) {
    if (!csrfToken) {
      throw new ApiError(
        400,
        'Hãy tải lại phiên trước khi tiếp tục.',
        'invalid_csrf',
      );
    }

    headers.set('X-CSRF-TOKEN', csrfToken);
  }

  const response = await fetch(`/api${path}`, {
    ...options,
    method,
    headers,
    credentials: 'include',
    cache: 'no-store',
  });

  const text = await response.text();
  let data: unknown;

  if (text) {
    try {
      data = JSON.parse(text);
    } catch {
      throw new ApiError(
        response.status,
        `API trả về phản hồi không hợp lệ (HTTP ${response.status}).`,
      );
    }
  }

  if (!response.ok) {
    const problem = data as ApiProblem | undefined;

    const validationMessage = problem?.errors
      ? Object.values(problem.errors).flat().join(' ')
      : undefined;

    const message =
      validationMessage ||
      problem?.message ||
      problem?.detail ||
      problem?.title ||
      `Yêu cầu thất bại (HTTP ${response.status}).`;

    throw new ApiError(response.status, message, problem?.code);
  }

  return data as T;
}