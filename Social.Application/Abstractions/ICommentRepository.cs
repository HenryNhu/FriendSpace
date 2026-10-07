using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.Abstractions
{
    public interface ICommentRepository
    {
        Task AddAsync(Comment comment, CancellationToken cancellationToken = default);

        Task<Comment?> GetByIdAsync(Guid postId, Guid commentId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Comment>> GetPageAsync(Guid postId, int skip, int take, CancellationToken cancellationToken = default);

        Task UpdateContentAsync(Comment comment, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(Guid postId, Guid commentId, Guid currentUserId, CancellationToken cancellationToken = default);
    }
}
