using Microsoft.EntityFrameworkCore;
using Npgsql;
using Social.Application.Abstractions;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Infrastructure.Persistence.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly SocialDbContext _dbContext;

        public UserProfileRepository(SocialDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return _dbContext.UserProfiles
                                .AsNoTracking()
                                .SingleOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
        }

        public async Task<UserProfile?> SaveAsync(UserProfile profile, CancellationToken cancellationToken = default)
        {
            var updatedAt = DateTimeOffset.UtcNow;

            try
            {
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"""
                        INSERT INTO "user_profiles" ("UserId", "DisplayName", "Bio", "CreatedAt", "UpdatedAt")
                        VALUES
                            (
                                {profile.UserId},
                                {profile.DisplayName},
                                {profile.Bio},
                                {profile.CreatedAt},
                                NULL
                            )
                        ON CONFLICT ("UserId")
                        DO UPDATE SET
                            "DisplayName" = EXCLUDED."DisplayName",
                            "Bio" = EXCLUDED."Bio",
                            "UpdatedAt" = {updatedAt}
                        WHERE
                            "user_profiles"."DisplayName" <> EXCLUDED."DisplayName"
                            OR "user_profiles"."Bio" <> EXCLUDED."Bio";
                    """, cancellationToken);
            }
            catch (PostgresException exception)
                when (
                    exception.SqlState == PostgresErrorCodes.ForeignKeyViolation &&
                    exception.ConstraintName == "fk_user_profiles_users_user_id")
            {
                return null;
            }

            return await GetByUserIdAsync(profile.UserId, cancellationToken);
        }

        public async Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            var ids = userIds.Distinct().ToArray();

            if (ids.Length == 0)
            {
                return new Dictionary<Guid, string>();
            }

            return await _dbContext.UserProfiles
                                    .AsNoTracking()
                                    .Where(profile => ids.Contains(profile.UserId))
                                    .Select(profile => new
                                    {
                                        profile.UserId,
                                        profile.DisplayName
                                    })
                                    .ToDictionaryAsync(
                                        profile => profile.UserId,
                                        profile => profile.DisplayName,
                                        cancellationToken);
        }
        public async Task<IReadOnlyDictionary<Guid, Social.Application.Abstractions.UserCard>> GetCardsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            var ids = userIds.Distinct().ToArray();
            var names = await GetDisplayNamesAsync(ids, cancellationToken);
            var avatars = await _dbContext.MediaAssets.AsNoTracking()
                .Where(m => m.IsAvatar && ids.Contains(m.OwnerId))
                .Select(m => new { m.OwnerId, m.Id }).ToListAsync(cancellationToken);
            var avatarIds = avatars.ToDictionary(m => m.OwnerId, m => m.Id);
            return names.ToDictionary(p => p.Key, p => new Social.Application.Abstractions.UserCard(
                p.Value, avatarIds.TryGetValue(p.Key, out var imageId) ? $"/api/media/{imageId}" : null));
        }

        public async Task<IReadOnlyList<Social.Application.UserProfiles.UserProfileResponse>> SearchAsync(string query, int skip, int take, CancellationToken cancellationToken = default)
        {
            var profiles = _dbContext.UserProfiles.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(query))
            {
                // Escape LIKE metacharacters: a typed percent sign is literal text.
                var escaped = query.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
                profiles = profiles.Where(p => EF.Functions.ILike(p.DisplayName, $"%{escaped}%", "\\"));
            }
            var page = await profiles.OrderBy(p => p.DisplayName).ThenBy(p => p.UserId)
                .Skip(skip).Take(take).ToListAsync(cancellationToken);
            var cards = await GetCardsAsync(page.Select(p => p.UserId), cancellationToken);
            return page.Select(p => new Social.Application.UserProfiles.UserProfileResponse
            {
                UserId = p.UserId,
                DisplayName = p.DisplayName,
                Bio = p.Bio,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                AvatarUrl = cards.GetValueOrDefault(p.UserId)?.AvatarUrl
            }).ToList();
        }
    }
}
