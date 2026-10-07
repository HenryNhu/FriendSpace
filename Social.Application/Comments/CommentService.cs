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

        public async Task<CommentResponse?> CreateAsync(Guid postId, Guid authorId, string content, CancellationToken cancellationToken = default)
        {
            var post = await _postRepository.GetByIdAsync(postId, cancellationToken);

            if (post is null)
            {
                return null;
            }

            var comment = new Comment(postId, authorId, content);

            await _commentRepository.AddAsync(comment, cancellationToken);

            return new CommentResponse
            {
                Id = comment.Id,
                PostId = comment.PostId,
                AuthorId = comment.AuthorId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }
    }
}
