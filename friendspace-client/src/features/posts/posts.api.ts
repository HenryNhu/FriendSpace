import { request } from '../../shared/api/http-client';
import type { Post } from './posts.types';

export const postsApi = {
  getPosts(page: number): Promise<Post[]> {
    return request<Post[]>(
      `/posts?pageNumber=${page}&pageSize=10`,
    );
  },

  createPost(content: string): Promise<Post> {
    return request<Post>('/posts', {
      method: 'POST',
      body: JSON.stringify({ content }),
    });
  },
};