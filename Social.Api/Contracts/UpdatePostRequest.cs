using Social.Domain.Entities;
namespace Social.Api.Contracts;

public class UpdatePostRequest
{
    public string Content { get; set; } = string.Empty;
    // An older client sending only Content must not accidentally publish a
    // private post or remove all of its images.
    public PostVisibility? Visibility { get; set; }
    public Guid[]? MediaIds { get; set; }
}
