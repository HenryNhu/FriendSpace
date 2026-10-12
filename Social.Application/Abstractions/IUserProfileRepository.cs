using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.Abstractions
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<UserProfile?> SaveAsync(UserProfile profile, CancellationToken cancellationToken = default);

        Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    }
}
