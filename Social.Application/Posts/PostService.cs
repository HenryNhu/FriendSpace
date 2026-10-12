using Social.Application.Abstractions;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.Posts
{
    public class PostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserProfileRepository _userProfileRepository;

        public PostService(IPostRepository postRepository, IUserProfileRepository userProfileRepository)
        {
            _postRepository = postRepository;
            _userProfileRepository = userProfileRepository;
        }

        #region Response
        private static PostResponse ToResponse(Post post)
        {
            return new PostResponse
            {
                Id = post.Id,
                AuthorId = post.AuthorId,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
            };
        }

        private async Task<PostResponse> ToResponseAsync(Post post, CancellationToken cancellationToken)
        {
            var responses = await ToResponsesAsync(new[] { post }, cancellationToken);

            return responses[0];
        }

        private async Task<IReadOnlyList<PostResponse>> ToResponsesAsync(IReadOnlyList<Post> posts, CancellationToken cancellationToken)
        {
            var displayNames = await _userProfileRepository.GetDisplayNamesAsync(posts.Select(post => post.AuthorId), cancellationToken);

            return posts.Select(post =>
            {
                displayNames.TryGetValue(post.AuthorId, out var authorDisplayName);

                return new PostResponse
                {
                    Id = post.Id,
                    AuthorId = post.AuthorId,
                    AuthorDisplayName = authorDisplayName,
                    Content = post.Content,
                    CreatedAt = post.CreatedAt,
                    UpdatedAt = post.UpdatedAt
                };
            }).ToList();
        }
        #endregion

        public async Task<PostResponse> CreateAsync(Guid authorId, string content, CancellationToken cancellationToken = default)
        {
            var post = new Post(authorId, content);

            await _postRepository.AddAsync(post, cancellationToken);

            //return ToResponse(post);
            return await ToResponseAsync(post, cancellationToken);
        }

        public async Task<PostResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var post = await _postRepository.GetByIdAsync(id, cancellationToken);

            if (post is null)
            {
                return null;
            }

            //return ToResponse(post);
            return post is null ? null : await ToResponseAsync(post, cancellationToken);
        }

        public async Task<IReadOnlyList<PostResponse>> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Số trang phải lớn hơn hoặc bằng 1.");
            }

            if (pageSize < 1 || pageSize > 50)
            {
                throw new ArgumentException("Số bài mỗi trang phải từ 1 đến 50.");
            }

            long skip = ((long)pageNumber - 1) * pageSize;

            if (skip > int.MaxValue)
            {
                throw new ArgumentException("Số trang quá lớn.");
            }

            var posts = await _postRepository.GetPageAsync((int)skip, pageSize, cancellationToken);

            //return posts.Select(ToResponse).ToList();
            return await ToResponsesAsync(posts, cancellationToken);
        }

        public async Task<PostResponse?> UpdateContentAsync(Guid id, Guid currentUserId, string content, CancellationToken cancellationToken = default)
        {
            var post = await _postRepository.GetByIdAsync(id, cancellationToken);

            if (post is null || post.AuthorId != currentUserId)
            {
                return null;
            }

            post.UpdateContent(content);

            await _postRepository.UpdateContentAsync(post, cancellationToken);

            return ToResponse(post);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            return await _postRepository.DeleteAsync(id, currentUserId, cancellationToken);
        }
    }
}
