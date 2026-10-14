using Microsoft.EntityFrameworkCore;
using Social.Application.Abstractions;
using Social.Application.PostLikes;
using Social.Domain.Entities;
namespace Social.Infrastructure.Persistence.Repositories;

public class PostLikeRepository(SocialDbContext db) : IPostLikeRepository
{
    public async Task<bool> EnsureLikedAsync(PostLike like, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var rows = await db.Posts.FromSqlInterpolated($"SELECT * FROM posts WHERE \"Id\" = {like.PostId} FOR SHARE").AsNoTracking().ToListAsync(ct);
        if (rows.Count == 0 || !await db.Posts.VisibleTo(db, like.UserId).AnyAsync(p => p.Id == like.PostId, ct)) return false;
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO post_likes (\"PostId\", \"UserId\", \"CreatedAt\") VALUES ({like.PostId}, {like.UserId}, {like.CreatedAt}) ON CONFLICT (\"PostId\", \"UserId\") DO NOTHING", ct);
        await transaction.CommitAsync(ct);
        return true;
    }
    public async Task<bool> UnlikeAsync(Guid postId, Guid userId, CancellationToken ct = default)
    {
        var visiblePosts = db.Posts.VisibleTo(db, userId);
        if (!await visiblePosts.AnyAsync(p => p.Id == postId, ct)) return false;
        await db.PostLikes.Where(l => l.PostId == postId && l.UserId == userId &&
            visiblePosts.Any(p => p.Id == l.PostId)).ExecuteDeleteAsync(ct);
        return true;
    }
    public Task<PostLikeSummaryResponse?> GetSummaryAsync(Guid postId, Guid? me, CancellationToken ct = default) =>
        db.Posts.AsNoTracking().VisibleTo(db, me).Where(p => p.Id == postId).Select(p => new PostLikeSummaryResponse
        {
            PostId = p.Id,
            LikeCount = db.PostLikes.LongCount(l => l.PostId == p.Id),
            IsLikedByMe = me.HasValue && db.PostLikes.Any(l => l.PostId == p.Id && l.UserId == me.Value)
        }).SingleOrDefaultAsync(ct);
}
