using Social.Domain.Entities;

namespace Social.Application.Abstractions;

public interface IMediaRepository
{
    Task AddAsync(MediaAsset asset, CancellationToken cancellationToken = default);
    Task<MediaAsset?> FindVisibleAsync(Guid id, Guid? viewerId, CancellationToken cancellationToken = default);
    Task SetAvatarAsync(Guid ownerId, Guid assetId, CancellationToken cancellationToken = default);
    Task RemoveAvatarAsync(Guid ownerId, CancellationToken cancellationToken = default);
    // Implementations lock/recheck orphan status and delete the blob before its database record.
    Task<bool> DeleteUnattachedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);
    Task<int> PurgeExpiredAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default);
}
