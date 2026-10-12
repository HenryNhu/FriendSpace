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
    }
}
