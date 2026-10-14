namespace Social.Application.Friendships;

public record FriendshipResponse(Guid Id, Guid OtherUserId, string DisplayName, string? AvatarUrl, string State);
public record FriendshipStateResponse(Guid? Id, string State);
