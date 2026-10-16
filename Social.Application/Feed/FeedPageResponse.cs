using Social.Application.Posts;

namespace Social.Application.Feed;

public sealed record FeedPageResponse(IReadOnlyList<PostResponse> Items, string? NextCursor, string? TopCursor, bool HasMore, bool HasFriends);

public sealed record FeedUpdatesResponse(bool HasNewPosts);
