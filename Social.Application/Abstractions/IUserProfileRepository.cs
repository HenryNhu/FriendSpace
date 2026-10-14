using Social.Application.UserProfiles;
using Social.Domain.Entities;
namespace Social.Application.Abstractions;

public record UserCard(string DisplayName, string? AvatarUrl);
public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfile?> SaveAsync(UserProfile profile, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, UserCard>> GetCardsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserProfileResponse>> SearchAsync(string query, int skip, int take, CancellationToken cancellationToken = default);
}
