using Social.Domain.Entities;
namespace Social.Application.Posts;

public record MediaResponse(Guid Id, string Url);
public class PostResponse
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string? AuthorDisplayName { get; set; }
    public string? AuthorAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public PostVisibility Visibility { get; set; }
    public IReadOnlyList<MediaResponse> Images { get; set; } = [];
}
