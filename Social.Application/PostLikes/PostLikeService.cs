using Social.Application.Abstractions;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.PostLikes
{
    public class PostLikeService
    {
        private readonly IPostLikeRepository _postLikeRepository;

        public PostLikeService(IPostLikeRepository postLikeRepository)
        {
            _postLikeRepository = postLikeRepository;
        }

        public Task<bool> LikeAsync(Guid postId, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            var like = new PostLike(postId, currentUserId);

            return _postLikeRepository.EnsureLikedAsync(like, cancellationToken);
        }

        public Task UnlikeAsync(Guid postId, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            return _postLikeRepository.UnlikeAsync(postId, currentUserId, cancellationToken);
        }

        public Task<PostLikeSummaryResponse?> GetSummaryAsync(Guid postId, Guid? currentUserId, CancellationToken cancellationToken = default)
        {
            return _postLikeRepository.GetSummaryAsync(postId, currentUserId, cancellationToken);
        }
    }
}
