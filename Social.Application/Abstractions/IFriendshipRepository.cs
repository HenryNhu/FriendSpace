using Social.Domain.Entities;
namespace Social.Application.Abstractions;

public interface IFriendshipRepository
{
    Task<bool> UserExistsAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Friendship>> GetMineAsync(Guid userId, CancellationToken ct);
    Task<Friendship?> GetWithAsync(Guid userId, Guid otherUserId, CancellationToken ct);
    Task<Friendship> CreateAsync(Guid userId, Guid otherUserId, CancellationToken ct);
    Task<bool> AcceptAsync(Guid id, Guid currentUserId, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, Guid currentUserId, CancellationToken ct);
}
