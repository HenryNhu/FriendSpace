using System.Security.Claims;
using Social.Api.Authentication;
using Social.Api.Contracts;
using Social.Application.Comments;
namespace Social.Api.Endpoint;

public static class CommentEndpoints
{
    public static RouteGroupBuilder MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts/{postId:guid}/comments").WithTags("Comments").AddEndpointFilter<BusinessEndpointFilter>();
        group.MapPost("", CreateAsync).RequireAuthorization();
        group.MapGet("", GetPageAsync);
        group.MapGet("/{commentId:guid}", GetByIdAsync).WithName("Comments.GetById");
        group.MapPut("/{commentId:guid}/content", UpdateContentAsync).RequireAuthorization();
        group.MapDelete("/{commentId:guid}", DeleteAsync).RequireAuthorization();
        return group;
    }
    private static async Task<IResult> CreateAsync(Guid postId, CreateCommentRequest request, CommentService service, ClaimsPrincipal user, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        var comment = await service.CreateAsync(postId, me, request.Content, ct);
        return comment is null ? BusinessResults.NotFound() : Results.CreatedAtRoute("Comments.GetById", new { postId, commentId = comment.Id }, comment);
    }
    private static async Task<IResult> GetPageAsync(Guid postId, CommentService service, ClaimsPrincipal user, CancellationToken ct, int pageNumber = 1, int pageSize = 20)
    {
        var comments = await service.GetPageAsync(postId, pageNumber, pageSize, user.TryGetCurrentUserId(out var me) ? me : null, ct);
        return comments is null ? BusinessResults.NotFound() : Results.Ok(comments);
    }
    private static async Task<IResult> GetByIdAsync(Guid postId, Guid commentId, CommentService service, ClaimsPrincipal user, CancellationToken ct)
    {
        var comment = await service.GetByIdAsync(postId, commentId, user.TryGetCurrentUserId(out var me) ? me : null, ct);
        return comment is null ? BusinessResults.NotFound() : Results.Ok(comment);
    }
    private static async Task<IResult> UpdateContentAsync(Guid postId, Guid commentId, UpdateCommentRequest request, CommentService service, ClaimsPrincipal user, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        var comment = await service.UpdateContentAsync(postId, commentId, me, request.Content, ct);
        return comment is null ? BusinessResults.NotFound() : Results.Ok(comment);
    }
    private static async Task<IResult> DeleteAsync(Guid postId, Guid commentId, CommentService service, ClaimsPrincipal user, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        return await service.DeleteAsync(postId, commentId, me, ct) ? Results.NoContent() : BusinessResults.NotFound();
    }
}
