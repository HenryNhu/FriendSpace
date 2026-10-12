using Social.Application.Abstractions;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.UserProfiles
{
    public class UserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepository;

        public UserProfileService(IUserProfileRepository userProfileRepository)
        {
            _userProfileRepository = userProfileRepository;
        }

        private static UserProfileResponse ToResponse(UserProfile profile)
        {
            return new UserProfileResponse
            {
                UserId = profile.UserId,
                DisplayName = profile.DisplayName,
                Bio = profile.Bio,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }

        public async Task<UserProfileResponse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);

            return profile is null ? null : ToResponse(profile);
        }

        public async Task<UserProfileResponse?> SaveAsync(Guid currentUserId, string displayName, string? bio, CancellationToken cancellationToken = default)
        {
            var requestedProfile = new UserProfile(currentUserId, displayName,bio);

            var savedProfile = await _userProfileRepository.SaveAsync(requestedProfile, cancellationToken);

            return savedProfile is null ? null : ToResponse(savedProfile);
        }
    }
}
