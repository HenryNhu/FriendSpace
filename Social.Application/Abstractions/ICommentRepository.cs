using Social.Domain.Entities;
namespace Social.Application.Abstractions;

public interface ICommentRepository
{
    Task<bool> AddAsync(Comment comment, CancellationToken ct = default);
    Task<Comment?> GetByIdAsync(Guid postId, Guid commentId, Guid? viewerId, CancellationToken ct = default);
    Task<IReadOnlyList<Comment>> GetPageAsync(Guid postId, int skip, int take, Guid? viewerId, CancellationToken ct = default);
    Task<bool> UpdateContentAsync(Comment comment, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid postId, Guid commentId, Guid userId, CancellationToken ct = default);
}
