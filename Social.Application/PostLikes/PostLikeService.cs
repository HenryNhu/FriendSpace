using Social.Application.Abstractions;
using Social.Domain.Entities;
namespace Social.Application.PostLikes;

public class PostLikeService(IPostLikeRepository repository)
{
    public Task<bool> LikeAsync(Guid postId, Guid me, CancellationToken ct = default) => repository.EnsureLikedAsync(new PostLike(postId, me), ct);
    public Task<bool> UnlikeAsync(Guid postId, Guid me, CancellationToken ct = default) => repository.UnlikeAsync(postId, me, ct);
    public Task<PostLikeSummaryResponse?> GetSummaryAsync(Guid postId, Guid? me, CancellationToken ct = default) => repository.GetSummaryAsync(postId, me, ct);
}
