using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.Posts
{
    public class PostResponse
    {
        public Guid Id { get; set; }

        public Guid AuthorId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }
    }
}
