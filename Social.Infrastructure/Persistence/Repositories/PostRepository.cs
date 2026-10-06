using Microsoft.EntityFrameworkCore;
using Social.Application.Abstractions;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Infrastructure.Persistence.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly SocialDbContext _dbContext;

        public PostRepository(SocialDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Post post, CancellationToken cancellationToken = default)
        {
            _dbContext.Posts.Add(post);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.Posts
                .AsNoTracking()
                .SingleOrDefaultAsync(post => post.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Post>> GetPageAsync(int skip, int take, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Posts
                .AsNoTracking()
                .OrderByDescending(post => post.CreatedAt)
                .ThenByDescending(post => post.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateContentAsync(Post post, CancellationToken cancellationToken = default)
        {
            _dbContext.Posts.Attach(post);

            var entry = _dbContext.Entry(post);

            entry.Property(p => p.Content).IsModified = true;
            entry.Property(p => p.UpdatedAt).IsModified = true;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            var affectedRows = await _dbContext.Posts
                .Where(post => 
                    post.Id == id &&
                    post.AuthorId == currentUserId)
                .ExecuteDeleteAsync(cancellationToken);

            return affectedRows > 0;
        }
    }
}
