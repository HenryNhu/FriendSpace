using Social.Application.Abstractions;

namespace Social.Application.Feed;

public sealed class FeedService(IFeedRepository repository)
{
    public async Task<FeedPageResponse> GetPageAsync(Guid? viewerId, string? cursor, int pageSize, CancellationToken ct = default)
    {
        if (pageSize is < 1 or > 50)
            throw new ArgumentException("Số bài mỗi lần tải phải từ 1 đến 50.");

        var before = FeedCursor.Decode(cursor);
        // Read one extra row to know whether another page exists.
        var rows = await repository.GetPageAsync(viewerId, before, pageSize + 1, ct);
        var items = rows.Take(pageSize).ToList();
        var hasMore = rows.Count > pageSize;
        var next = hasMore
            ? FeedCursor.Encode(new(items[^1].CreatedAt, items[^1].Id))
            : null;
        var top = items.Count > 0
            ? FeedCursor.Encode(new(items[0].CreatedAt, items[0].Id))
            : null;
        var hasFriends = viewerId.HasValue && await repository.HasFriendsAsync(viewerId.Value, ct);

        return new(items, next, top, hasMore, hasFriends);
    }

    public async Task<FeedUpdatesResponse> GetUpdatesAsync(Guid? viewerId, string? after, CancellationToken ct = default) 
        => new(await repository.HasNewPostsAsync(viewerId, FeedCursor.Decode(after), ct));
}
