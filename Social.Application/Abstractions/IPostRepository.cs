using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.Abstractions
{
    public interface IPostRepository
    {
        Task AddAsync(Post post, CancellationToken cancellationToken = default);
        
        Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task<IReadOnlyList<Post>> GetPageAsync(int skip, int take, CancellationToken cancellationToken = default);

        Task UpdateContentAsync(Post post, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
    }
}
