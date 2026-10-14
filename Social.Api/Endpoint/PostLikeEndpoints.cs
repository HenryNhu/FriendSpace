using System.Security.Claims;
using Social.Api.Authentication;
using Social.Application.PostLikes;
namespace Social.Api.Endpoint;

public static class PostLikeEndpoints
{
    public static RouteGroupBuilder MapPostLikeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts/{postId:guid}/likes").WithTags("Post Likes").AddEndpointFilter<BusinessEndpointFilter>();
        group.MapPut("/me", LikeAsync).RequireAuthorization();
        group.MapDelete("/me", UnlikeAsync).RequireAuthorization();
        group.MapGet("", GetSummaryAsync);
        return group;
    }
    private static async Task<IResult> LikeAsync(Guid postId, ClaimsPrincipal user, PostLikeService service, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        return await service.LikeAsync(postId, me, ct) ? Results.NoContent() : BusinessResults.NotFound();
    }
    private static async Task<IResult> UnlikeAsync(Guid postId, ClaimsPrincipal user, PostLikeService service, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        return await service.UnlikeAsync(postId, me, ct) ? Results.NoContent() : BusinessResults.NotFound();
    }
    private static async Task<IResult> GetSummaryAsync(Guid postId, ClaimsPrincipal user, PostLikeService service, CancellationToken ct)
    {
        var summary = await service.GetSummaryAsync(postId, user.TryGetCurrentUserId(out var me) ? me : null, ct);
        return summary is null ? BusinessResults.NotFound() : Results.Ok(summary);
    }
}
