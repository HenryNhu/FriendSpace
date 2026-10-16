using Social.Application.Feed;
using Social.Application.Posts;

namespace Social.Application.Abstractions;

public interface IFeedRepository
{
    Task<IReadOnlyList<PostResponse>> GetPageAsync(Guid? viewerId, FeedPosition? before, int take, CancellationToken ct = default);

    Task<bool> HasNewPostsAsync(Guid? viewerId, FeedPosition? after, CancellationToken ct = default);

    Task<bool> HasFriendsAsync(Guid viewerId, CancellationToken ct = default);
}
