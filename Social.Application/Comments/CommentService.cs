using Social.Application.Abstractions;
using Social.Application.Common;
using Social.Domain.Entities;
namespace Social.Application.Comments;

public class CommentService(IPostRepository posts, ICommentRepository comments, IUserProfileRepository profiles)
{
    private async Task<IReadOnlyList<CommentResponse>> MapAsync(IReadOnlyList<Comment> rows, CancellationToken ct)
    {
        var cards = await profiles.GetCardsAsync(rows.Select(c => c.AuthorId), ct);
        return rows.Select(c => new CommentResponse
        {
            Id = c.Id,
            PostId = c.PostId,
            AuthorId = c.AuthorId,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            AuthorDisplayName = cards.GetValueOrDefault(c.AuthorId)?.DisplayName,
            AuthorAvatarUrl = cards.GetValueOrDefault(c.AuthorId)?.AvatarUrl
        }).ToList();
    }
    public async Task<CommentResponse?> CreateAsync(Guid postId, Guid me, string content, CancellationToken ct = default)
    {
        var comment = new Comment(postId, me, content);
        return await comments.AddAsync(comment, ct) ? (await MapAsync([comment], ct))[0] : null;
    }
    public async Task<CommentResponse?> GetByIdAsync(Guid postId, Guid id, Guid? viewerId, CancellationToken ct = default)
    {
        var row = await comments.GetByIdAsync(postId, id, viewerId, ct);
        return row is null ? null : (await MapAsync([row], ct))[0];
    }
    public async Task<IReadOnlyList<CommentResponse>?> GetPageAsync(Guid postId, int pageNumber, int pageSize, Guid? viewerId, CancellationToken ct = default)
    {
        var skip = Paging.Skip(pageNumber, pageSize);
        if (await posts.GetByIdAsync(postId, viewerId, ct) is null) return null;
        return await MapAsync(await comments.GetPageAsync(postId, skip, pageSize, viewerId, ct), ct);
    }
    public async Task<CommentResponse?> UpdateContentAsync(Guid postId, Guid id, Guid me, string content, CancellationToken ct = default)
    {
        var row = await comments.GetByIdAsync(postId, id, me, ct);
        if (row is null || row.AuthorId != me) return null;
        row.UpdateContent(content);
        return await comments.UpdateContentAsync(row, ct) ? (await MapAsync([row], ct))[0] : null;
    }
    public Task<bool> DeleteAsync(Guid postId, Guid id, Guid me, CancellationToken ct = default) => comments.DeleteAsync(postId, id, me, ct);
}
