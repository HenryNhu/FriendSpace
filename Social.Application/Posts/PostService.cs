using Social.Application.Abstractions;
using Social.Application.Common;
using Social.Domain.Entities;
namespace Social.Application.Posts;

public class PostService(IPostRepository posts, IUserProfileRepository profiles)
{
    private static IReadOnlyList<Guid> ValidateMedia(IReadOnlyList<Guid>? ids)
    {
        ids ??= [];
        if (ids.Count > 4 || ids.Any(id => id == Guid.Empty) || ids.Distinct().Count() != ids.Count)
            throw new InvalidMediaException("Mỗi bài có tối đa 4 ảnh khác nhau.");
        return ids;
    }
    public async Task<PostResponse> CreateAsync(Guid authorId, string content, PostVisibility visibility, IReadOnlyList<Guid>? mediaIds, CancellationToken ct = default)
    {
        var post = new Post(authorId, content, visibility);
        await posts.AddAsync(post, ValidateMedia(mediaIds), ct);
        return (await ToResponsesAsync([post], ct))[0];
    }
    public async Task<PostResponse?> GetByIdAsync(Guid id, Guid? viewerId, CancellationToken ct = default)
    {
        var post = await posts.GetByIdAsync(id, viewerId, ct);
        return post is null ? null : (await ToResponsesAsync([post], ct))[0];
    }
    public async Task<IReadOnlyList<PostResponse>> GetPageAsync(int pageNumber, int pageSize, Guid? viewerId, Guid? authorId = null, CancellationToken cancellationToken = default)
    {
        var page = await posts.GetPageAsync(Paging.Skip(pageNumber, pageSize), pageSize, viewerId, authorId, cancellationToken);
        return await ToResponsesAsync(page, cancellationToken);
    }
    public async Task<PostResponse?> UpdateContentAsync(Guid id, Guid userId, string content, PostVisibility visibility, IReadOnlyList<Guid>? mediaIds, CancellationToken ct = default)
    {
        Post.Validate(content, visibility);
        var post = await posts.UpdateContentAsync(id, userId, content, visibility, ValidateMedia(mediaIds), ct);
        return post is null ? null : (await ToResponsesAsync([post], ct))[0];
    }
    public Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default) => posts.DeleteAsync(id, userId, ct);
    private async Task<IReadOnlyList<PostResponse>> ToResponsesAsync(IReadOnlyList<Post> items, CancellationToken ct)
    {
        var cards = await profiles.GetCardsAsync(items.Select(p => p.AuthorId), ct);
        var images = await posts.GetImagesAsync(items.Select(p => p.Id), ct);
        return items.Select(p => new PostResponse
        {
            Id = p.Id,
            AuthorId = p.AuthorId,
            Content = p.Content,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            Visibility = p.Visibility,
            AuthorDisplayName = cards.GetValueOrDefault(p.AuthorId)?.DisplayName,
            AuthorAvatarUrl = cards.GetValueOrDefault(p.AuthorId)?.AvatarUrl,
            Images = images.GetValueOrDefault(p.Id) ?? []
        }).ToList();
    }
}
