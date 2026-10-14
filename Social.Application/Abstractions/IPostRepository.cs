using Social.Application.Posts;
using Social.Domain.Entities;
namespace Social.Application.Abstractions;

public interface IPostRepository
{
    Task AddAsync(Post post, IReadOnlyList<Guid> mediaIds, CancellationToken cancellationToken = default);
    Task<Post?> GetByIdAsync(Guid id, Guid? viewerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetPageAsync(int skip, int take, Guid? viewerId, Guid? authorId, CancellationToken cancellationToken = default);
    Task<Post?> UpdateContentAsync(Guid id, Guid currentUserId, string content, PostVisibility visibility, IReadOnlyList<Guid> mediaIds, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<MediaResponse>>> GetImagesAsync(IEnumerable<Guid> postIds, CancellationToken cancellationToken = default);
}
