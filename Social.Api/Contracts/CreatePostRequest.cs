using Social.Domain.Entities;
namespace Social.Api.Contracts;

public class CreatePostRequest
{
    public string Content { get; set; } = string.Empty;
    public PostVisibility Visibility { get; set; } = PostVisibility.Public;
    public Guid[] MediaIds { get; set; } = [];
}
