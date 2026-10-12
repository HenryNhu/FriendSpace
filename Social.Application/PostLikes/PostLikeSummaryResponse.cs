using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.PostLikes
{
    public class PostLikeSummaryResponse
    {
        public Guid PostId { get; set; }

        public long LikeCount { get; set; }

        public bool IsLikedByMe { get; set; }
    }
}
