using Social.Api.Authentication;
using Social.Api.Contracts;
using Social.Application.Comments;
using Social.Application.Posts;
using System.Security.Claims;

namespace Social.Api.Endpoint
{
    public static class PostEndpoints
    {
        public static RouteGroupBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/posts").WithTags("Posts");

            group.MapPost("", CreateAsync);
            group.MapGet("", GetPageAsync);
            group.MapGet("/{id:guid}", GetByIdAsync);
            group.MapPut("/{id:guid}/content", UpdateContentAsync);
            group.MapDelete("/{id:guid}", DeleteAsync).RequireAuthorization();
            group.MapPost("/{id:guid}/comments", CreateCommentAsync).RequireAuthorization();

            return group;
        }

        private static async Task<IResult> CreateAsync(CreatePostRequest request, PostService service, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            if(!user.TryGetCurrentUserId(out var currentUserID))
            {
                return Results.Unauthorized();
            }

            try
            {
                var post = await service.CreateAsync(currentUserID, request.Content, cancellationToken);

                return Results.Ok(post);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        private static async Task<IResult> GetByIdAsync(Guid id, PostService service, CancellationToken cancellationToken)
        {
            var post = await service.GetByIdAsync(id, cancellationToken);

            if (post is null)
            {
                return Results.NotFound(new
                {
                    message = "Không tìm thấy bài viết."
                });
            }

            return Results.Ok(post);
        }

        private static async Task<IResult> GetPageAsync(PostService service, CancellationToken cancellationToken, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var posts = await service.GetPageAsync(pageNumber, pageSize, cancellationToken);

                return Results.Ok(posts);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        private static async Task<IResult> UpdateContentAsync(Guid id, UpdatePostRequest request, PostService service, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            if (!user.TryGetCurrentUserId(out var currentUserId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var post = await service.UpdateContentAsync(id, currentUserId, request.Content, cancellationToken);

                if (post is null)
                {
                    return Results.NotFound(new
                    {
                        message = "Không tìm thấy bài viết."
                    });
                }

                return Results.Ok(post);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        private static async Task<IResult> DeleteAsync(Guid id, PostService service, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            if(!user.TryGetCurrentUserId(out var currentUserId))
            {
                return Results.Unauthorized();
            }

            var deleted = await service.DeleteAsync(id, currentUserId, cancellationToken);

            if (!deleted)
            {
                return Results.NotFound(new
                {
                    message = "Không tìm thấy bài viết."
                });
            }

            return Results.NoContent();
        }
    }
}
