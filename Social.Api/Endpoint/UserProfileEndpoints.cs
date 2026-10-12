using Social.Api.Authentication;
using Social.Api.Contracts;
using Social.Application.UserProfiles;
using System.Security.Claims;

namespace Social.Api.Endpoint
{
    public static class UserProfileEndpoints
    {
        public static RouteGroupBuilder MapUserProfileEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup("/api/profiles")
                .WithTags("User Profiles");

            group.MapGet("/me", GetMineAsync)
                .RequireAuthorization();

            group.MapPut("/me", SaveMineAsync)
                .RequireAuthorization();

            group.MapGet("/{userId:guid}", GetByUserIdAsync);

            return group;
        }

        private static async Task<IResult> GetMineAsync(
            ClaimsPrincipal user,
            UserProfileService service,
            CancellationToken cancellationToken)
        {
            if (!user.TryGetCurrentUserId(out var currentUserId))
            {
                return Results.Unauthorized();
            }

            return await GetByUserIdAsync(
                currentUserId,
                service,
                cancellationToken);
        }

        private static async Task<IResult> GetByUserIdAsync(
            Guid userId,
            UserProfileService service,
            CancellationToken cancellationToken)
        {
            var profile = await service.GetByUserIdAsync(
                userId,
                cancellationToken);

            if (profile is null)
            {
                return Results.NotFound(new
                {
                    message = "Không tìm thấy hồ sơ."
                });
            }

            return Results.Ok(profile);
        }

        private static async Task<IResult> SaveMineAsync(
            SaveUserProfileRequest request,
            ClaimsPrincipal user,
            UserProfileService service,
            CancellationToken cancellationToken)
        {
            if (!user.TryGetCurrentUserId(out var currentUserId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var profile = await service.SaveAsync(
                    currentUserId,
                    request.DisplayName,
                    request.Bio,
                    cancellationToken);

                if (profile is null)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(profile);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new
                {
                    message = exception.Message
                });
            }
        }
    }
}
