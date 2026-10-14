using Social.Application.Abstractions;
using Social.Application.Common;
using Social.Domain.Entities;
namespace Social.Application.UserProfiles;

public class UserProfileService(IUserProfileRepository profiles)
{
    private async Task<UserProfileResponse> ToResponseAsync(UserProfile profile, CancellationToken ct)
    {
        var cards = await profiles.GetCardsAsync([profile.UserId], ct);
        return new UserProfileResponse
        {
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            Bio = profile.Bio,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt,
            AvatarUrl = cards.GetValueOrDefault(profile.UserId)?.AvatarUrl
        };
    }
    public async Task<UserProfileResponse?> GetByUserIdAsync(Guid id, CancellationToken ct = default)
    {
        var profile = await profiles.GetByUserIdAsync(id, ct);
        return profile is null ? null : await ToResponseAsync(profile, ct);
    }
    public async Task<UserProfileResponse?> SaveAsync(Guid id, string displayName, string? bio, CancellationToken ct = default)
    {
        var profile = await profiles.SaveAsync(new UserProfile(id, displayName, bio), ct);
        return profile is null ? null : await ToResponseAsync(profile, ct);
    }
    public Task<IReadOnlyList<UserProfileResponse>> SearchAsync(string? query, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var text = query?.Trim() ?? string.Empty;
        if (text.Length > 80) throw new ArgumentException("Từ khóa tìm kiếm quá dài.");
        return profiles.SearchAsync(text, Paging.Skip(pageNumber, pageSize), pageSize, ct);
    }
}
