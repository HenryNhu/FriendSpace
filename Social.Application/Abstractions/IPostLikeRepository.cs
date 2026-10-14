using Social.Application.PostLikes;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.Abstractions
{
    public interface IPostLikeRepository
    {
        Task<bool> EnsureLikedAsync(PostLike like, CancellationToken cancellationToken = default);

        Task<bool> UnlikeAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);

        Task<PostLikeSummaryResponse?> GetSummaryAsync(Guid postId, Guid? currentUserId, CancellationToken cancellationToken = default);
    }
}
