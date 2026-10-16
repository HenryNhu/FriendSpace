using Microsoft.EntityFrameworkCore;
using Social.Application.Feed;
using Social.Domain.Entities;

namespace Social.Infrastructure.Persistence;

public static class FeedQuery
{
    public static IQueryable<Post> ForFeed(this IQueryable<Post> posts, SocialDbContext db, Guid? viewerId)
    {
        var visible = posts.VisibleTo(db, viewerId);
        if (!viewerId.HasValue) return visible;

        var id = viewerId.Value;
        return visible.Where(p => p.AuthorId == id || db.Friendships.Any(f =>
            f.Status == FriendshipStatus.Accepted &&
            ((f.UserLowId == id && f.UserHighId == p.AuthorId) ||
             (f.UserHighId == id && f.UserLowId == p.AuthorId))));
    }

    public static IQueryable<Post> Before(this IQueryable<Post> posts, FeedPosition position) =>
        posts.Where(p => EF.Functions.LessThan(
            ValueTuple.Create(p.CreatedAt, p.Id),
            ValueTuple.Create(position.CreatedAt, position.Id)));

    public static IQueryable<Post> After(this IQueryable<Post> posts, FeedPosition position) =>
        posts.Where(p => EF.Functions.GreaterThan(
            ValueTuple.Create(p.CreatedAt, p.Id),
            ValueTuple.Create(position.CreatedAt, position.Id)));
}
