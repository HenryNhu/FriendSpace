using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Domain.Entities
{
    public class Comment
    {
        public const int MaxContentLength = 2000;

        public Guid Id { get; private set; }

        public Guid PostId { get; private set; }

        public Guid AuthorId { get; private set; }

        public string Content { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public Comment(Guid postId, Guid authorId, string content)
        {
            if(postId == Guid.Empty)
            {
                throw new ArgumentException("Bình luận phải thuộc một bài viết.", nameof(postId));
            }

            if(authorId == Guid.Empty)
            {
                throw new ArgumentException("Bình luận phải có tác giả.", nameof(authorId));
            }

            Id = Guid.NewGuid();
            PostId = postId;
            AuthorId = authorId;
            Content = NormalizeContent(content);
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateContent(string content)
        {
            var nomilizedContent = NormalizeContent(content);

            if(Content == nomilizedContent)
            {
                return;
            }

            Content = nomilizedContent;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        private static string NormalizeContent(string content)
            {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Nội dung bình luận không được để trống.", nameof(content));
            }

            var normalizedContent = content.Trim();

            if (normalizedContent.Length > MaxContentLength)
            {
                throw new ArgumentException($"Bình luận không được vượt quá {MaxContentLength} ký tự.", nameof(content));
            }

            return normalizedContent;
        }
    }
}
