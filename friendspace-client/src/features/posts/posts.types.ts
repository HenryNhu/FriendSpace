export type Visibility = 0 | 1 | 2;
export interface PostImage {
  id: string;
  url: string;
}
export interface Post {
  id: string;
  authorId: string;
  authorDisplayName: string | null;
  authorAvatarUrl: string | null;
  content: string;
  visibility: Visibility;
  images: PostImage[];
  createdAt: string;
  updatedAt: string | null;
}
export interface PostInput {
  content: string;
  visibility: Visibility;
  mediaIds: string[];
}
