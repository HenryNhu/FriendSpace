using Microsoft.EntityFrameworkCore;
using Social.Application.Abstractions;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Infrastructure.Persistence.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly SocialDbContext _dbContext;

        public CommentRepository(SocialDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
        {
            _dbContext.Comments.Add(comment);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<Comment?> GetByIdAsync(Guid postId, Guid commentId, CancellationToken cancellationToken = default)
        {
            return _dbContext.Comments
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    comment =>
                        comment.PostId == postId &&
                        comment.Id == commentId,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<Comment>> GetPageAsync(Guid postId, int skip, int take, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Comments
                .AsNoTracking()
                .Where(comment => comment.PostId == postId)
                .OrderBy(comment => comment.CreatedAt)
                .ThenBy(comment => comment.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateContentAsync(Comment comment, CancellationToken cancellationToken = default)
        {
            _dbContext.Comments.Attach(comment);

            var entry = _dbContext.Entry(comment);

            entry.Property(c => c.Content).IsModified = true;
            entry.Property(c => c.UpdatedAt).IsModified = true;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid postId, Guid commentId, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            var affectedRows = await _dbContext.Comments
                .Where(comment =>
                    comment.PostId == postId &&
                    comment.Id == commentId &&
                    (
                        comment.AuthorId == currentUserId ||
                        _dbContext.Posts.Any(post =>
                            post.Id == postId &&
                            post.AuthorId == currentUserId)
                    ))
                .ExecuteDeleteAsync(cancellationToken);

            return affectedRows > 0;
        }
    }
}
