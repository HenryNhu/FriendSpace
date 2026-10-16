using Microsoft.EntityFrameworkCore;
using Social.Application.Abstractions;
using Social.Application.Feed;
using Social.Application.Posts;
using Social.Domain.Entities;

namespace Social.Infrastructure.Persistence.Repositories;

public sealed class FeedRepository(SocialDbContext db) : IFeedRepository
{
    public async Task<IReadOnlyList<PostResponse>> GetPageAsync(Guid? viewerId, FeedPosition? before, int take, CancellationToken ct = default)
    {
        var query = db.Posts.AsNoTracking().ForFeed(db, viewerId);
        if (before is not null) query = query.Before(before);

        return await Project(query.OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id).Take(take)).ToListAsync(ct);
    }

    public Task<bool> HasNewPostsAsync(Guid? viewerId, FeedPosition? after, CancellationToken ct = default)
    {
        var query = db.Posts.AsNoTracking().ForFeed(db, viewerId);
        if (after is not null) query = query.After(after);
        return query.AnyAsync(ct);
    }

    public Task<bool> HasFriendsAsync(Guid viewerId, CancellationToken ct = default) =>
        db.Friendships.AnyAsync(f => f.Status == FriendshipStatus.Accepted &&
            (f.UserLowId == viewerId || f.UserHighId == viewerId), ct);

    // Project only data needed by the feed. EF translates this before execution.
    public IQueryable<PostResponse> Project(IQueryable<Post> query) => query.Select(p => new PostResponse
    {
        Id = p.Id,
        AuthorId = p.AuthorId,
        Content = p.Content,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Visibility = p.Visibility,
        AuthorDisplayName = db.UserProfiles.Where(profile => profile.UserId == p.AuthorId)
            .Select(profile => profile.DisplayName).FirstOrDefault(),
        AuthorAvatarUrl = db.MediaAssets.Where(m => m.OwnerId == p.AuthorId && m.IsAvatar)
            .Select(m => "/api/media/" + m.Id).FirstOrDefault(),
        Images = db.MediaAssets.Where(m => m.PostId == p.Id)
            .OrderBy(m => m.CreatedAt).ThenBy(m => m.Id)
            .Select(m => new MediaResponse(m.Id, "/api/media/" + m.Id)).ToList()
    });
}
