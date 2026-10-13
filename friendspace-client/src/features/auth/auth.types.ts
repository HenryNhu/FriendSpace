export interface User {
  id: string;
  email: string | null;
}

export interface Session {
  user: User | null;
  csrfToken: string;
}