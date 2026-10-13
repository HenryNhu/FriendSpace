import type { Post, Profile, Session } from './types';

export class ApiError extends Error {
  status: number;
  code?: string;

  constructor(status: number, message: string, code?: string) {
    super(message);
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
let sessionRequest: Promise<Session> | undefined;

async function request<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const method = (options.method ?? 'GET').toUpperCase();
  const headers = new Headers(options.headers);

  headers.set('Accept', 'application/json');

  if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json');
  }

  if (!['GET', 'HEAD', 'OPTIONS'].includes(method)) {
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
    headers,
    credentials: 'include',
    cache: 'no-store',
  });

  const text = await response.text();
  let data: unknown;

  try {
    data = text ? JSON.parse(text) : undefined;
  } catch {
    throw new ApiError(
      response.status,
      `API trả về phản hồi không hợp lệ (HTTP ${response.status}).`,
    );
  }

  if (!response.ok) {
    const problem = (data ?? {}) as ApiProblem;

    const validationErrors = Object.values(problem.errors ?? {})
      .flat()
      .join(' ');

    throw new ApiError(
      response.status,
      validationErrors ||
        problem.message ||
        problem.detail ||
        problem.title ||
        `HTTP ${response.status}`,
      problem.code,
    );
  }

  return data as T;
}

function getSession(): Promise<Session> {
  if (!sessionRequest) {
    sessionRequest = request<Session>('/auth/session')
      .then(session => {
        csrfToken = session.csrfToken;
        return session;
      })
      .finally(() => {
        sessionRequest = undefined;
      });
  }

  return sessionRequest;
}

export const api = {
  getSession,

  register: (email: string, password: string) =>
    request<void>('/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),

  async login(email: string, password: string) {
    await request<void>(
      '/auth/login?useCookies=true&useSessionCookies=false',
      {
        method: 'POST',
        body: JSON.stringify({ email, password }),
      },
    );

    csrfToken = null;
  },

  async logout() {
    await request<void>('/auth/logout', {
      method: 'POST',
      body: JSON.stringify({}),
    });

    csrfToken = null;
  },

  getProfile: () => request<Profile>('/profiles/me'),

  saveProfile: (displayName: string, bio: string) =>
    request<Profile>('/profiles/me', {
      method: 'PUT',
      body: JSON.stringify({ displayName, bio }),
    }),

  getPosts: (page: number) =>
    request<Post[]>(`/posts?pageNumber=${page}&pageSize=10`),

  createPost: (content: string) =>
    request<Post>('/posts', {
      method: 'POST',
      body: JSON.stringify({ content }),
    }),
};