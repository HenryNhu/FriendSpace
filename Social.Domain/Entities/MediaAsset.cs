namespace Social.Domain.Entities;

public class MediaAsset
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string BlobName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? PostId { get; set; }
    public bool IsAvatar { get; set; }
}
