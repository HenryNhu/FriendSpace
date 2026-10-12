using Social.Api.Authentication;
using Social.Application.PostLikes;
using System.Security.Claims;

namespace Social.Api.Endpoint
{
    public static class PostLikeEndpoints
    {
        public static RouteGroupBuilder MapPostLikeEndpoints(
        this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup("/api/posts/{postId:guid}/likes")
                .WithTags("Post Likes");

            group.MapPut("/me", LikeAsync)
                .RequireAuthorization();

            group.MapDelete("/me", UnlikeAsync)
                .RequireAuthorization();

            group.MapGet("", GetSummaryAsync);

            return group;
        }

        private static async Task<IResult> LikeAsync(Guid postId, ClaimsPrincipal user, PostLikeService service, CancellationToken cancellationToken)
        {
            if (!user.TryGetCurrentUserId(out var currentUserId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var succeeded = await service.LikeAsync(postId, currentUserId, cancellationToken);

                if (!succeeded)
                {
                    return Results.NotFound(new
                    {
                        message = "Không tìm thấy bài viết."
                    });
                }

                return Results.NoContent();
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        private static async Task<IResult> UnlikeAsync(Guid postId, ClaimsPrincipal user, PostLikeService service, CancellationToken cancellationToken)
        {
            if (!user.TryGetCurrentUserId(out var currentUserId))
            {
                return Results.Unauthorized();
            }

            await service.UnlikeAsync(postId, currentUserId, cancellationToken);

            return Results.NoContent();
        }

        private static async Task<IResult> GetSummaryAsync(Guid postId, ClaimsPrincipal user, PostLikeService service, CancellationToken cancellationToken)
        {
            Guid? currentUserId = user.TryGetCurrentUserId(out var userId) ? userId : null;

            var summary = await service.GetSummaryAsync(postId, currentUserId, cancellationToken);

            if (summary is null)
            {
                return Results.NotFound(new
                {
                    message = "Không tìm thấy bài viết."
                });
            }

            return Results.Ok(summary);
        }
    }
}
