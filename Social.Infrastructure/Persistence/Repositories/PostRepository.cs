using Microsoft.EntityFrameworkCore;
using Social.Application.Abstractions;
using Social.Application.Common;
using Social.Application.Posts;
using Social.Domain.Entities;
namespace Social.Infrastructure.Persistence.Repositories;

public class PostRepository(SocialDbContext db) : IPostRepository
{
    public async Task AddAsync(Post post, IReadOnlyList<Guid> mediaIds, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var assets = await LockMediaAsync(mediaIds, ct);
        ValidateMedia(assets, mediaIds, post.AuthorId, post.Id);
        db.Posts.Add(post);
        foreach (var asset in assets) asset.PostId = post.Id;
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public Task<Post?> GetByIdAsync(Guid id, Guid? viewerId, CancellationToken ct = default) =>
        db.Posts.AsNoTracking().VisibleTo(db, viewerId).SingleOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Post>> GetPageAsync(int skip, int take, Guid? viewerId, Guid? authorId, CancellationToken ct = default)
    {
        var query = db.Posts.AsNoTracking().VisibleTo(db, viewerId);
        if (authorId.HasValue) query = query.Where(p => p.AuthorId == authorId.Value);
        return await query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
    }

    public async Task<Post?> UpdateContentAsync(Guid id, Guid userId, string content, PostVisibility visibility, IReadOnlyList<Guid> mediaIds, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // Lock the post first: two edits cannot attach a different final set of images simultaneously.
        var rows = await db.Posts.FromSqlInterpolated($"SELECT * FROM posts WHERE \"Id\" = {id} AND \"AuthorId\" = {userId} FOR UPDATE").ToListAsync(ct);
        var post = rows.SingleOrDefault();
        if (post is null) return null;
        var oldIds = await db.MediaAssets.Where(m => m.PostId == id).Select(m => m.Id).ToListAsync(ct);
        var assets = await LockMediaAsync(mediaIds.Concat(oldIds).Distinct().ToArray(), ct);
        var selected = assets.Where(m => mediaIds.Contains(m.Id)).ToList();
        ValidateMedia(selected, mediaIds, userId, id);
        post.UpdateContent(content, visibility);
        foreach (var asset in assets) asset.PostId = mediaIds.Contains(asset.Id) ? id : null;
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return post;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await db.Posts.Where(p => p.Id == id && p.AuthorId == userId).ExecuteDeleteAsync(ct) > 0;

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<MediaResponse>>> GetImagesAsync(IEnumerable<Guid> postIds, CancellationToken ct = default)
    {
        var ids = postIds.Distinct().ToArray();
        var rows = await db.MediaAssets.AsNoTracking().Where(m => m.PostId.HasValue && ids.Contains(m.PostId.Value))
            .OrderBy(m => m.CreatedAt).ThenBy(m => m.Id).Select(m => new { m.Id, PostId = m.PostId!.Value }).ToListAsync(ct);
        return rows.GroupBy(m => m.PostId).ToDictionary(g => g.Key,
            g => (IReadOnlyList<MediaResponse>)g.Select(m => new MediaResponse(m.Id, $"/api/media/{m.Id}")).ToList());
    }

    private async Task<List<MediaAsset>> LockMediaAsync(IEnumerable<Guid> mediaIds, CancellationToken ct)
    {
        var ids = mediaIds.Distinct().ToArray();
        if (ids.Length == 0) return [];
        var assets = await db.MediaAssets.FromSqlInterpolated(
            $"SELECT * FROM media_assets WHERE \"Id\" = ANY({ids}) ORDER BY \"Id\" FOR UPDATE").ToListAsync(ct);
        foreach (var asset in assets) await db.Entry(asset).ReloadAsync(ct);
        return assets;
    }

    private static void ValidateMedia(IReadOnlyList<MediaAsset> assets, IReadOnlyList<Guid> ids, Guid ownerId, Guid postId)
    {
        if (assets.Count != ids.Count || assets.Any(m => m.OwnerId != ownerId || m.IsAvatar || (m.PostId.HasValue && m.PostId != postId) || (!m.PostId.HasValue && m.CreatedAt < DateTimeOffset.UtcNow.AddHours(-24))))
            throw new InvalidMediaException("Ảnh không tồn tại, đã được sử dụng hoặc không thuộc tài khoản của bạn.");
    }
}
