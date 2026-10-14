namespace Social.Application.Abstractions;

public interface IMediaStorage
{
    Task UploadAsync(string blobName, Stream content, string contentType,
        CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string blobName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobName, CancellationToken cancellationToken = default);
}
