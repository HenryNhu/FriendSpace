export interface Post {
  id: string;
  authorId: string;
  authorDisplayName: string | null;
  content: string;
  createdAt: string;
  updatedAt: string | null;
}