using Social.Application.Abstractions;
using Social.Domain.Entities;
namespace Social.Application.Friendships;

public class FriendshipService(IFriendshipRepository repository, IUserProfileRepository profiles)
{
    private static string State(Friendship row, Guid userId) => row.Status == FriendshipStatus.Accepted
        ? "accepted" : row.RequestedById == userId ? "outgoing" : "incoming";
    private async Task<IReadOnlyList<FriendshipResponse>> MapAsync(IReadOnlyList<Friendship> rows, Guid me, CancellationToken ct)
    {
        var others = rows.Select(r => r.UserLowId == me ? r.UserHighId : r.UserLowId).ToArray();
        var cards = await profiles.GetCardsAsync(others, ct);
        return rows.Select(r =>
        {
            var other = r.UserLowId == me ? r.UserHighId : r.UserLowId;
            var card = cards.GetValueOrDefault(other);
            return new FriendshipResponse(r.Id, other, card?.DisplayName ?? "", card?.AvatarUrl, State(r, me));
        }).ToList();
    }
    public async Task<IReadOnlyList<FriendshipResponse>> GetMineAsync(Guid me, CancellationToken ct) =>
        await MapAsync(await repository.GetMineAsync(me, ct), me, ct);
    public async Task<FriendshipStateResponse?> GetWithAsync(Guid me, Guid other, CancellationToken ct)
    {
        if (me == other) return new(null, "self");
        if (!await repository.UserExistsAsync(other, ct)) return null;
        var row = await repository.GetWithAsync(me, other, ct);
        return row is null ? new(null, "none") : new(row.Id, State(row, me));
    }
    public async Task<FriendshipResponse?> CreateAsync(Guid me, Guid other, CancellationToken ct)
    {
        if (me == other || other == Guid.Empty) throw new ArgumentException("Không thể gửi lời mời cho chính mình.");
        if (!await repository.UserExistsAsync(other, ct)) return null;
        return (await MapAsync([await repository.CreateAsync(me, other, ct)], me, ct))[0];
    }
    public Task<bool> AcceptAsync(Guid id, Guid me, CancellationToken ct) => repository.AcceptAsync(id, me, ct);
    public Task<bool> DeleteAsync(Guid id, Guid me, CancellationToken ct) => repository.DeleteAsync(id, me, ct);
}
