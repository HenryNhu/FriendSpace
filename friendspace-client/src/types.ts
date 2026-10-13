export interface User {
  id: string;
  email: string | null;
}

export interface Session {
  user: User | null;
  csrfToken: string;
}

export interface Profile {
  userId: string;
  displayName: string;
  bio: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface Post {
  id: string;
  authorId: string;
  authorDisplayName: string | null;
  content: string;
  createdAt: string;
  updatedAt: string | null;
}export interface User {
  id: string;
  email: string | null;
}

export interface Session {
  user: User | null;
  csrfToken: string;
}

export interface Profile {
  userId: string;
  displayName: string;
  bio: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface Post {
  id: string;
  authorId: string;
  authorDisplayName: string | null;
  content: string;
  createdAt: string;
  updatedAt: string | null;
}