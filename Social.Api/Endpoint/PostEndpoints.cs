using System.Security.Claims;
using Social.Api.Authentication;
using Social.Api.Contracts;
using Social.Application.Posts;
namespace Social.Api.Endpoint;

public static class PostEndpoints
{
    public static RouteGroupBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts").WithTags("Posts").AddEndpointFilter<BusinessEndpointFilter>();
        group.MapPost("", CreateAsync).RequireAuthorization();
        group.MapGet("", GetPageAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPut("/{id:guid}/content", UpdateContentAsync).RequireAuthorization();
        group.MapDelete("/{id:guid}", DeleteAsync).RequireAuthorization();
        return group;
    }
    private static async Task<IResult> CreateAsync(CreatePostRequest request, PostService service, ClaimsPrincipal user, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        return Results.Ok(await service.CreateAsync(me, request.Content, request.Visibility, request.MediaIds, ct));
    }
    private static async Task<IResult> GetByIdAsync(Guid id, PostService service, ClaimsPrincipal user, CancellationToken ct)
    {
        var post = await service.GetByIdAsync(id, user.TryGetCurrentUserId(out var me) ? me : null, ct);
        return post is null ? BusinessResults.NotFound() : Results.Ok(post);
    }
    private static async Task<IResult> GetPageAsync(PostService service, ClaimsPrincipal user, CancellationToken ct, int pageNumber = 1, int pageSize = 10) =>
        Results.Ok(await service.GetPageAsync(pageNumber, pageSize, user.TryGetCurrentUserId(out var me) ? me : null, cancellationToken: ct));
    private static async Task<IResult> UpdateContentAsync(Guid id, UpdatePostRequest request, PostService service, ClaimsPrincipal user, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        if (request.Visibility is null || request.MediaIds is null)
            throw new ArgumentException("Khi sửa bài, cần gửi quyền xem và danh sách ảnh cuối cùng.");
        var post = await service.UpdateContentAsync(id, me, request.Content, request.Visibility.Value, request.MediaIds, ct);
        return post is null ? BusinessResults.NotFound() : Results.Ok(post);
    }
    private static async Task<IResult> DeleteAsync(Guid id, PostService service, ClaimsPrincipal user, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        return await service.DeleteAsync(id, me, ct) ? Results.NoContent() : BusinessResults.NotFound();
    }
}
