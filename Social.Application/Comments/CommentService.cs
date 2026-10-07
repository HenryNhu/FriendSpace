using Social.Application.Abstractions;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.Comments
{
    public class CommentService
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;

        public CommentService(IPostRepository postRepository, ICommentRepository commentRepository)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
        }

        private static CommentResponse ToResponse(Comment comment)
        {
            return new CommentResponse
            {
                Id = comment.Id,
                PostId = comment.PostId,
                AuthorId = comment.AuthorId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }

        public async Task<CommentResponse?> CreateAsync(Guid postId, Guid currentUserId, string content, CancellationToken cancellationToken = default)
        {
            var post = await _postRepository.GetByIdAsync(postId, cancellationToken);

            if (post is null)
            {
                return null;
            }

            var comment = new Comment(postId, currentUserId, content);

            await _commentRepository.AddAsync(comment, cancellationToken);

            return ToResponse(comment);
        }

        public async Task<CommentResponse?> GetByIdAsync(Guid postId, Guid commentId, CancellationToken cancellationToken = default)
        {
            var comment = await _commentRepository.GetByIdAsync(postId, commentId, cancellationToken);

            return comment is null ? null : ToResponse(comment);
        }

        public async Task<IReadOnlyList<CommentResponse>?> GetPageAsync(Guid postId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Số trang phải lớn hơn hoặc bằng 1.");
            }

            if (pageSize < 1 || pageSize > 50)
            {
                throw new ArgumentException("Số bình luận mỗi trang phải từ 1 đến 50.");
            }

            long skip = ((long)pageNumber - 1) * pageSize;

            if (skip > int.MaxValue)
            {
                throw new ArgumentException("Số trang quá lớn.");
            }

            var post = await _postRepository.GetByIdAsync(postId, cancellationToken);

            if (post is null)
            {
                return null;
            }

            var comments = await _commentRepository.GetPageAsync(postId, (int)skip, pageSize, cancellationToken);

            return comments.Select(ToResponse).ToList();
        }

        public async Task<CommentResponse?> UpdateContentAsync(Guid postId, Guid commentId, Guid currentUserId, string content, CancellationToken cancellationToken = default)
        {
            var comment = await _commentRepository.GetByIdAsync(postId, commentId, cancellationToken);

            if (comment is null || comment.AuthorId != currentUserId)
            {
                return null;
            }

            comment.UpdateContent(content);

            await _commentRepository.UpdateContentAsync(comment, cancellationToken);

            return ToResponse(comment);
        }

        public async Task<bool> DeleteAsync(Guid postId, Guid commentId, Guid currentUserId, CancellationToken cancellationToken = default)
            {
            return await _commentRepository.DeleteAsync(postId, commentId, currentUserId, cancellationToken);
        }

    }
}
