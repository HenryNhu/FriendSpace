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
    }
}
