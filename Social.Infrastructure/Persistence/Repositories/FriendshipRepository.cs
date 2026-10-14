using Microsoft.EntityFrameworkCore;
using Npgsql;
using Social.Application.Abstractions;
using Social.Application.Common;
using Social.Domain.Entities;
namespace Social.Infrastructure.Persistence.Repositories;

public class FriendshipRepository(SocialDbContext db) : IFriendshipRepository
{
    public Task<bool> UserExistsAsync(Guid id, CancellationToken ct) => db.Users.AnyAsync(u => u.Id == id, ct);
    public async Task<IReadOnlyList<Friendship>> GetMineAsync(Guid me, CancellationToken ct) =>
        await db.Friendships.AsNoTracking().Where(f => f.UserLowId == me || f.UserHighId == me)
            .OrderByDescending(f => f.CreatedAt).ThenBy(f => f.Id).ToListAsync(ct);
    public Task<Friendship?> GetWithAsync(Guid me, Guid other, CancellationToken ct)
    {
        var (low, high) = Friendship.Pair(me, other);
        return db.Friendships.AsNoTracking().SingleOrDefaultAsync(f => f.UserLowId == low && f.UserHighId == high, ct);
    }
    public async Task<Friendship> CreateAsync(Guid me, Guid other, CancellationToken ct)
    {
        var (low, high) = Friendship.Pair(me, other);
        var row = new Friendship
        {
            Id = Guid.NewGuid(),
            UserLowId = low,
            UserHighId = high,
            RequestedById = me,
            Status = FriendshipStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Friendships.Add(row);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            db.Entry(row).State = EntityState.Detached;
            throw new FriendshipExistsException();
        }
        return row;
    }
    public async Task<bool> AcceptAsync(Guid id, Guid me, CancellationToken ct) =>
        await db.Friendships.Where(f => f.Id == id && f.Status == FriendshipStatus.Pending &&
            f.RequestedById != me && (f.UserLowId == me || f.UserHighId == me))
            .ExecuteUpdateAsync(setters => setters.SetProperty(f => f.Status, FriendshipStatus.Accepted)
                .SetProperty(f => f.AcceptedAt, DateTimeOffset.UtcNow), ct) > 0;
    public async Task<bool> DeleteAsync(Guid id, Guid me, CancellationToken ct) =>
        await db.Friendships.Where(f => f.Id == id && (f.UserLowId == me || f.UserHighId == me))
            .ExecuteDeleteAsync(ct) > 0;
}
