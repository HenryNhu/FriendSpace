using Social.Api.Authentication;
using Social.Api.Contracts;
using Social.Application.Comments;
using System.Security.Claims;

namespace Social.Api.Endpoint
{
    public static class CommentEndpoints
    {
        private const string GetByIdRouteName = "Comments.GetById";

        public static RouteGroupBuilder MapCommentEndpoints(
            this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup("/api/posts/{postId:guid}/comments")
                .WithTags("Comments");

            group.MapPost("", CreateAsync)
                .RequireAuthorization();

            group.MapGet("", GetPageAsync);

            group.MapGet("/{commentId:guid}", GetByIdAsync)
                .WithName(GetByIdRouteName);

            group.MapPut("/{commentId:guid}/content", UpdateContentAsync)
                .RequireAuthorization();

            group.MapDelete("/{commentId:guid}", DeleteAsync)
                .RequireAuthorization();

            return group;
        }

        private static async Task<IResult> CreateAsync(Guid postId, CreateCommentRequest request, CommentService service, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            if (!user.TryGetCurrentUserId(out var currentUserId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var comment = await service.CreateAsync(postId, currentUserId, request.Content, cancellationToken);

                if (comment is null)
                {
                    return Results.NotFound(new
                    {
                        message = "Không tìm thấy bài viết."
                    });
                }

                return Results.CreatedAtRoute(GetByIdRouteName, new { postId, commentId = comment.Id },comment);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        private static async Task<IResult> GetPageAsync(Guid postId, CommentService service, CancellationToken cancellationToken, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var comments = await service.GetPageAsync(postId, pageNumber, pageSize, cancellationToken);

                return comments is null
                    ? Results.NotFound(new
                    {
                        message = "Không tìm thấy bài viết."
                    })
                    : Results.Ok(comments);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        private static async Task<IResult> GetByIdAsync(Guid postId, Guid commentId, CommentService service, CancellationToken cancellationToken)
        {
            var comment = await service.GetByIdAsync(postId, commentId, cancellationToken);

            return comment is null
                ? Results.NotFound(new
                {
                    message = "Không tìm thấy bình luận."
                })
                : Results.Ok(comment);
        }

        private static async Task<IResult> UpdateContentAsync(Guid postId, Guid commentId, UpdateCommentRequest request, CommentService service, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            if (!user.TryGetCurrentUserId(out var currentUserId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var comment = await service.UpdateContentAsync(postId, commentId, currentUserId, request.Content, cancellationToken);

                return comment is null
                    ? Results.NotFound(new
                    {
                        message = "Không tìm thấy bình luận."
                    })
                    : Results.Ok(comment);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        private static async Task<IResult> DeleteAsync(Guid postId, Guid commentId, CommentService service, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            if (!user.TryGetCurrentUserId(out var currentUserId))
            {
                return Results.Unauthorized();
            }

            var deleted = await service.DeleteAsync(postId, commentId, currentUserId, cancellationToken);

            return deleted
                ? Results.NoContent()
                : Results.NotFound(new
                {
                    message = "Không tìm thấy bình luận."
                });
        }
    }
}
