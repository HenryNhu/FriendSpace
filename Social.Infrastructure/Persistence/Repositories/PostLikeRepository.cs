using Microsoft.EntityFrameworkCore;
using Npgsql;
using Social.Application.Abstractions;
using Social.Application.PostLikes;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Infrastructure.Persistence.Repositories
{
    public class PostLikeRepository : IPostLikeRepository
    {
        private readonly SocialDbContext _dbContext;

        public PostLikeRepository(SocialDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> EnsureLikedAsync(PostLike like, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"""
                        INSERT INTO "post_likes" ("PostId", "UserId", "CreatedAt")
                        VALUES ({like.PostId}, {like.UserId}, {like.CreatedAt})
                        ON CONFLICT ("PostId", "UserId") DO NOTHING;
                    """, cancellationToken);

                return true;
            }
            catch (PostgresException exception) 
                when (exception.SqlState == PostgresErrorCodes.ForeignKeyViolation && 
                        exception.ConstraintName == "fk_post_likes_posts_post_id")
            {
                return false;
            }
        }

        public async Task UnlikeAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default)
        {
            await _dbContext.PostLikes
                                .Where(like =>
                                    like.PostId == postId &&
                                    like.UserId == userId)
                                .ExecuteDeleteAsync(cancellationToken);
        }

        public Task<PostLikeSummaryResponse?> GetSummaryAsync(Guid postId, Guid? currentUserId, CancellationToken cancellationToken = default)
        {
            return _dbContext.Posts
                                .AsNoTracking()
                                .Where(post => post.Id == postId)
                                .Select(post => new PostLikeSummaryResponse
                                {
                                    PostId = post.Id,

                                    LikeCount = _dbContext.PostLikes
                                                            .LongCount(like => like.PostId == post.Id),

                                    IsLikedByMe = currentUserId.HasValue && 
                                                    _dbContext.PostLikes.Any(like => like.PostId == post.Id && 
                                                                                like.UserId == currentUserId)
                                })
                                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
