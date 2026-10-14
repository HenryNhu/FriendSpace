using Social.Domain.Entities;
namespace Social.Infrastructure.Persistence;

public static class PostVisibilityQuery
{
    public static IQueryable<Post> VisibleTo(this IQueryable<Post> posts, SocialDbContext db, Guid? viewerId)
    {
        if (!viewerId.HasValue) return posts.Where(p => p.Visibility == PostVisibility.Public);
        var id = viewerId.Value;
        return posts.Where(p => p.Visibility == PostVisibility.Public || p.AuthorId == id ||
            (p.Visibility == PostVisibility.Friends && db.Friendships.Any(f =>
                f.Status == FriendshipStatus.Accepted &&
                ((f.UserLowId == id && f.UserHighId == p.AuthorId) ||
                 (f.UserHighId == id && f.UserLowId == p.AuthorId)))));
    }
}
