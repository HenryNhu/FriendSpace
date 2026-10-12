using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Domain.Entities
{
    public class PostLike
    {
        public Guid PostId { get; private set; }

        public Guid UserId { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        private PostLike()
        {
        }

        public PostLike(Guid postId, Guid userId)
        {
            if (postId == Guid.Empty)
            {
                throw new ArgumentException("Bài viết không hợp lệ.", nameof(postId));
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentException("Người dùng không hợp lệ.", nameof(userId));
            }

            PostId = postId;
            UserId = userId;
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
