import {
  request,
  setCsrfToken,
} from '../../shared/api/http-client';

import type { Session } from './auth.types';

let sessionRequest: Promise<Session> | undefined;

function getSession(): Promise<Session> {
  if (!sessionRequest) {
    sessionRequest = request<Session>('/auth/session')
      .then((session) => {
        setCsrfToken(session.csrfToken);

        return session;
      })
      .finally(() => {
        sessionRequest = undefined;
      });
  }

  return sessionRequest;
}

export const authApi = {
  getSession,

  register(email: string, password: string): Promise<void> {
    return request<void>('/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
  },

  async login(email: string, password: string): Promise<void> {
    await request<void>(
      '/auth/login?useCookies=true&useSessionCookies=false',
      {
        method: 'POST',
        body: JSON.stringify({ email, password }),
      },
    );

    setCsrfToken(null);
  },

  async logout(): Promise<void> {
    await request<void>('/auth/logout', {
      method: 'POST',
      body: JSON.stringify({}),
    });

    setCsrfToken(null);
  },
};