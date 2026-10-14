namespace Social.Domain.Entities;

public enum FriendshipStatus { Pending = 0, Accepted = 1 }
public class Friendship
{
    public Guid Id { get; set; }
    public Guid UserLowId { get; set; }
    public Guid UserHighId { get; set; }
    public Guid RequestedById { get; set; }
    public FriendshipStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public static (Guid Low, Guid High) Pair(Guid first, Guid second) =>
        string.CompareOrdinal(first.ToString("N"), second.ToString("N")) < 0
            ? (first, second) : (second, first);
}
