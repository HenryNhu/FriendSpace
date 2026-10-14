using Microsoft.EntityFrameworkCore;
using Social.Application.Abstractions;
using Social.Application.Media;
using Social.Domain.Entities;
using Social.Infrastructure.Persistence;

namespace Social.Infrastructure.Media;

public sealed class MediaRepository(SocialDbContext db, IMediaStorage storage) : IMediaRepository
{
    public async Task AddAsync(MediaAsset asset, CancellationToken cancellationToken = default)
    {
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<MediaAsset?> FindVisibleAsync(Guid id, Guid? viewerId, CancellationToken cancellationToken = default)
    {
        var visiblePosts = db.Posts.VisibleTo(db, viewerId).Select(post => post.Id);
        return db.MediaAssets.AsNoTracking().SingleOrDefaultAsync(asset => asset.Id == id &&
            (asset.IsAvatar ||
             (asset.PostId != null && visiblePosts.Contains(asset.PostId.Value)) ||
             (asset.PostId == null && asset.OwnerId == viewerId)), cancellationToken);
    }

    public async Task SetAvatarAsync(Guid ownerId, Guid assetId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        // A user-row lock serializes first-avatar, replacement and removal requests.
        var users = await db.Users.FromSqlInterpolated(
            $"SELECT * FROM \"AspNetUsers\" WHERE \"Id\" = {ownerId} FOR UPDATE").ToListAsync(cancellationToken);
        if (users.Count == 0) throw new MediaException("validation_error", "Không tìm thấy tài khoản.");
        // Lock both old and new assets in the same UUID order used by post
        // attachments, preventing crossed requests from acquiring reverse locks.
        var avatarAssets = await db.MediaAssets.FromSqlInterpolated(
            $"SELECT * FROM media_assets WHERE \"Id\" = {assetId} OR (\"OwnerId\" = {ownerId} AND \"IsAvatar\" = TRUE) ORDER BY \"Id\" FOR UPDATE")
            .ToListAsync(cancellationToken);
        foreach (var current in avatarAssets) await db.Entry(current).ReloadAsync(cancellationToken);
        var asset = avatarAssets.SingleOrDefault(current => current.Id == assetId);
        if (asset is null || asset.OwnerId != ownerId || asset.PostId != null || asset.IsAvatar)
            throw new MediaException("validation_error", "Ảnh không còn khả dụng để làm ảnh đại diện.");

        // Execute the old-avatar update before enabling the new one so the filtered
        // unique index is never violated, even within this transaction.
        await db.MediaAssets.Where(media => media.OwnerId == ownerId && media.IsAvatar)
            .ExecuteUpdateAsync(setters => setters.SetProperty(media => media.IsAvatar, false), cancellationToken);
        asset.IsAvatar = true;
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RemoveAvatarAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.Users.FromSqlInterpolated(
            $"SELECT * FROM \"AspNetUsers\" WHERE \"Id\" = {ownerId} FOR UPDATE").ToListAsync(cancellationToken);
        await db.MediaAssets.Where(media => media.OwnerId == ownerId && media.IsAvatar)
            .ExecuteUpdateAsync(setters => setters.SetProperty(media => media.IsAvatar, false), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<bool> DeleteUnattachedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var asset = await LockAssetAsync(id, cancellationToken);
        if (asset is null || asset.OwnerId != ownerId || asset.PostId != null || asset.IsAvatar) return false;
        await DeleteLockedAsync(asset, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<int> PurgeExpiredAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default)
    {
        var ids = await db.MediaAssets.AsNoTracking()
            .Where(asset => asset.PostId == null && !asset.IsAvatar && asset.CreatedAt < cutoff)
            .OrderBy(asset => asset.Id).Select(asset => asset.Id).Take(50).ToListAsync(cancellationToken);
        var deleted = 0;
        foreach (var id in ids)
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var asset = await LockAssetAsync(id, cancellationToken);
            // A post may have attached the asset after the candidate query.
            if (asset is null || asset.PostId != null || asset.IsAvatar || asset.CreatedAt >= cutoff) continue;
            await DeleteLockedAsync(asset, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            deleted++;
        }
        return deleted;
    }

    private async Task<MediaAsset?> LockAssetAsync(Guid id, CancellationToken cancellationToken)
    {
        var assets = await db.MediaAssets.FromSqlInterpolated(
            $"SELECT * FROM media_assets WHERE \"Id\" = {id} FOR UPDATE").ToListAsync(cancellationToken);
        var asset = assets.SingleOrDefault();
        // A preceding upload can already have this entity in the change tracker.
        // Reload under the row lock instead of trusting an earlier tracked value.
        if (asset is not null) await db.Entry(asset).ReloadAsync(cancellationToken);
        return asset;
    }

    private async Task DeleteLockedAsync(MediaAsset asset, CancellationToken cancellationToken)
    {
        // Keep the row on storage failure; cleanup can retry. If the process stops
        // after blob deletion, DeleteIfExists makes the next attempt safe.
        await storage.DeleteAsync(asset.BlobName, cancellationToken);
        db.MediaAssets.Remove(asset);
        await db.SaveChangesAsync(cancellationToken);
    }
}
