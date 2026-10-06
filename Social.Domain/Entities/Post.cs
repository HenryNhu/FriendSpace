using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Domain.Entities
{
    public class Post
    {
        public Guid Id { get; private set; }

        public Guid AuthorId { get; private set; }

        public string Content { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public Post(Guid authorId, string content)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentException("Bài viết phải có tác giả.", nameof(authorId));
            }
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Nội dung bài viết không được để trống.", nameof(content));
            }

            Id = Guid.NewGuid();
            AuthorId = authorId;
            Content = content.Trim();
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateContent(string content)
        {
            if(string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Nội dung bài viết không được để trống.", nameof(content));
            }

            Content = content.Trim();
        }
    }
}
