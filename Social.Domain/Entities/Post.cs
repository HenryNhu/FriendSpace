namespace Social.Domain.Entities;

public enum PostVisibility { Public = 0, Friends = 1, OnlyMe = 2 }

public class Post
{
    public const int MaxContentLength = 10000;
    public Guid Id { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public PostVisibility Visibility { get; private set; }
    private Post() { }
    public Post(Guid authorId, string content, PostVisibility visibility = PostVisibility.Public)
    {
        if (authorId == Guid.Empty) throw new ArgumentException("Bài viết phải có tác giả.");
        Validate(content, visibility);
        Id = Guid.NewGuid(); AuthorId = authorId; Content = content.Trim();
        Visibility = visibility; CreatedAt = DateTimeOffset.UtcNow;
    }
    public void UpdateContent(string content, PostVisibility visibility)
    {
        Validate(content, visibility);
        Content = content.Trim(); Visibility = visibility; UpdatedAt = DateTimeOffset.UtcNow;
    }
    public static void Validate(string? content, PostVisibility visibility)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Trim().Length > MaxContentLength)
            throw new ArgumentException($"Nội dung bài viết phải từ 1 đến {MaxContentLength} ký tự.");
        if (!Enum.IsDefined(visibility)) throw new ArgumentException("Quyền xem bài viết không hợp lệ.");
    }
}
