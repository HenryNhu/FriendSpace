using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.Comments
{
    public class CommentResponse
    {
        public Guid Id { get; set; }

        public Guid PostId { get; set; }

        public Guid AuthorId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
