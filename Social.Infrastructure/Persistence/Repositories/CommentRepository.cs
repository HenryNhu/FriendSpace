using Microsoft.EntityFrameworkCore;
using Social.Application.Abstractions;
using Social.Domain.Entities;
namespace Social.Infrastructure.Persistence.Repositories;

public class CommentRepository(SocialDbContext db) : ICommentRepository
{
    private IQueryable<Guid> VisiblePosts(Guid? viewerId) => db.Posts.VisibleTo(db, viewerId).Select(p => p.Id);
    public async Task<bool> AddAsync(Comment comment, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var rows = await db.Posts.FromSqlInterpolated($"SELECT * FROM posts WHERE \"Id\" = {comment.PostId} FOR SHARE")
            .AsNoTracking().ToListAsync(ct);
        if (rows.Count == 0 || !await VisiblePosts(comment.AuthorId).ContainsAsync(comment.PostId, ct)) return false;
        db.Comments.Add(comment);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }
    public Task<Comment?> GetByIdAsync(Guid postId, Guid id, Guid? viewerId, CancellationToken ct = default)
    {
        var visibleIds = VisiblePosts(viewerId);
        return db.Comments.AsNoTracking().Where(c => visibleIds.Contains(c.PostId))
            .SingleOrDefaultAsync(c => c.PostId == postId && c.Id == id, ct);
    }
    public async Task<IReadOnlyList<Comment>> GetPageAsync(Guid postId, int skip, int take, Guid? viewerId, CancellationToken ct = default)
    {
        var visibleIds = VisiblePosts(viewerId);
        return await db.Comments.AsNoTracking().Where(c => c.PostId == postId && visibleIds.Contains(c.PostId))
            .OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id).Skip(skip).Take(take).ToListAsync(ct);
    }
    public async Task<bool> UpdateContentAsync(Comment comment, CancellationToken ct = default)
    {
        var visibleIds = VisiblePosts(comment.AuthorId);
        return await db.Comments.Where(c => c.Id == comment.Id && c.PostId == comment.PostId && c.AuthorId == comment.AuthorId &&
            visibleIds.Contains(c.PostId))
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Content, comment.Content).SetProperty(c => c.UpdatedAt, comment.UpdatedAt), ct) > 0;
    }
    public async Task<bool> DeleteAsync(Guid postId, Guid id, Guid me, CancellationToken ct = default)
    {
        var visibleIds = VisiblePosts(me);
        return await db.Comments.Where(c => c.PostId == postId && c.Id == id && visibleIds.Contains(c.PostId) &&
            (c.AuthorId == me || db.Posts.Any(p => p.Id == postId && p.AuthorId == me))).ExecuteDeleteAsync(ct) > 0;
    }
}
