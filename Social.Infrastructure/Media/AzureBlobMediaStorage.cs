using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Social.Application.Abstractions;
using Social.Application.Media;

namespace Social.Infrastructure.Media;

public sealed class AzureBlobMediaStorage : IMediaStorage
{
    private readonly Lazy<BlobContainerClient> _container;

    public AzureBlobMediaStorage(string? serviceUri, string containerName, string? developmentConnectionString)
    {
        _container = new Lazy<BlobContainerClient>(() =>
        {
            // The required blob operations use this stable protocol version,
            // supported by Azure and local Azurite, independently of SDK updates.
            var options = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2023_11_03);
            options.Retry.Mode = RetryMode.Exponential;
            options.Retry.MaxRetries = 3;
            options.Retry.Delay = TimeSpan.FromMilliseconds(500);
            options.Retry.MaxDelay = TimeSpan.FromSeconds(5);
            options.Retry.NetworkTimeout = TimeSpan.FromSeconds(15);
            if (!string.IsNullOrWhiteSpace(developmentConnectionString))
                return new BlobServiceClient(developmentConnectionString, options).GetBlobContainerClient(containerName);
            if (!Uri.TryCreate(serviceUri, UriKind.Absolute, out var uri) || uri.Scheme != "https")
                throw new MediaException("media_unavailable", "Chưa cấu hình Azure Blob Storage cho ảnh.", 503);
            return new BlobServiceClient(uri, new DefaultAzureCredential(), options).GetBlobContainerClient(containerName);
        });
    }

    public async Task UploadAsync(string blobName, Stream content, string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var container = _container.Value;
            await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
            await EnsurePrivateAsync(container, cancellationToken);
            await container.GetBlobClient(blobName).UploadAsync(content, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType, CacheControl = "no-store" },
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
            }, cancellationToken);
        }
        catch (Exception exception) when (IsStorageFailure(exception)) { throw Unavailable(exception); }
    }

    public async Task<Stream?> OpenReadAsync(string blobName, CancellationToken cancellationToken = default)
    {
        try
        {
            var container = _container.Value;
            await EnsurePrivateAsync(container, cancellationToken);
            var response = await container.GetBlobClient(blobName).DownloadStreamingAsync(cancellationToken: cancellationToken);
            return response.Value.Content;
        }
        catch (RequestFailedException exception) when (exception.Status == 404) { return null; }
        catch (Exception exception) when (IsStorageFailure(exception)) { throw Unavailable(exception); }
    }

    public async Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.Value.GetBlobClient(blobName).DeleteIfExistsAsync(
                DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }
        catch (RequestFailedException exception) when (exception.Status == 404) { }
        catch (Exception exception) when (IsStorageFailure(exception)) { throw Unavailable(exception); }
    }

    private static async Task EnsurePrivateAsync(BlobContainerClient container, CancellationToken cancellationToken)
    {
        var properties = await container.GetPropertiesAsync(cancellationToken: cancellationToken);
        // A private container can be represented by null (no public-access header).
        if (properties.Value.PublicAccess is { } access && access != PublicAccessType.None)
            throw new MediaException("media_unavailable", "Container ảnh phải đặt quyền truy cập Private.", 503);
    }

    private static bool IsStorageFailure(Exception exception) =>
        exception is RequestFailedException or AuthenticationFailedException or HttpRequestException
            or FormatException or ArgumentException or InvalidOperationException;

    private static MediaException Unavailable(Exception exception) => new(
        "media_unavailable", "Kho ảnh hiện chưa sẵn sàng. Vui lòng thử lại sau.", 503, exception);
}
