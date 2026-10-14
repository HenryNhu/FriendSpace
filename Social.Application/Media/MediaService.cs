using Social.Application.Abstractions;
using Social.Domain.Entities;

namespace Social.Application.Media;

public sealed class MediaService(IMediaRepository repository, IMediaStorage storage)
{
    public const long MaxFileBytes = 5 * 1024 * 1024;

    public async Task<MediaUploadResponse> UploadAsync(Guid ownerId, Stream source,
        long declaredLength, bool asAvatar, CancellationToken cancellationToken = default)
    {
        if (declaredLength <= 0)
            throw new MediaException("image_type", "Hãy chọn một ảnh JPEG, PNG hoặc WebP.");
        if (declaredLength > MaxFileBytes)
            throw new MediaException("image_too_large", "Mỗi ảnh tối đa 5 MB.", 413);

        using var buffer = new MemoryStream();
        var bytes = new byte[81920];
        int read;
        while ((read = await source.ReadAsync(bytes.AsMemory(), cancellationToken)) > 0)
        {
            if (buffer.Length + read > MaxFileBytes)
                throw new MediaException("image_too_large", "Mỗi ảnh tối đa 5 MB.", 413);
            await buffer.WriteAsync(bytes.AsMemory(0, read), cancellationToken);
        }

        var (contentType, extension) = DetectImage(buffer.GetBuffer(), (int)buffer.Length);
        var asset = new MediaAsset
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            BlobName = $"images/{Guid.NewGuid():N}{extension}",
            ContentType = contentType,
            Size = buffer.Length,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Persist the staged record first: even an interrupted/failed upload remains
        // discoverable by cleanup. Never create an untracked blob.
        await repository.AddAsync(asset, cancellationToken);
        buffer.Position = 0;
        await storage.UploadAsync(asset.BlobName, buffer, contentType, cancellationToken);
        if (asAvatar)
            await repository.SetAvatarAsync(ownerId, asset.Id, cancellationToken);

        return new MediaUploadResponse(asset.Id, $"/api/media/{asset.Id}");
    }

    public async Task<MediaDownload?> ReadAsync(Guid id, Guid? viewerId,
        CancellationToken cancellationToken = default)
    {
        var asset = await repository.FindVisibleAsync(id, viewerId, cancellationToken);
        if (asset is null) return null;
        var stream = await storage.OpenReadAsync(asset.BlobName, cancellationToken);
        return stream is null ? null : new MediaDownload(stream, asset.ContentType);
    }

    public Task<bool> DeleteUnattachedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
        => repository.DeleteUnattachedAsync(id, ownerId, cancellationToken);

    public Task RemoveAvatarAsync(Guid ownerId, CancellationToken cancellationToken = default)
        => repository.RemoveAvatarAsync(ownerId, cancellationToken);

    private static (string ContentType, string Extension) DetectImage(byte[] data, int length)
    {
        if (length >= 3 && data[0] == 0xff && data[1] == 0xd8 && data[2] == 0xff)
            return ("image/jpeg", ".jpg");
        ReadOnlySpan<byte> png = [137, 80, 78, 71, 13, 10, 26, 10];
        if (length >= 8 && data.AsSpan(0, 8).SequenceEqual(png))
            return ("image/png", ".png");
        if (length >= 12 && data.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
            data.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return ("image/webp", ".webp");
        throw new MediaException("image_type", "Chỉ chấp nhận ảnh JPEG, PNG hoặc WebP. Không chấp nhận SVG.");
    }
}
