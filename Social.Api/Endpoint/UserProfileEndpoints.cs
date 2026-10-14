using System.Security.Claims;
using Social.Api.Authentication;
using Social.Api.Contracts;
using Social.Application.Posts;
using Social.Application.UserProfiles;
namespace Social.Api.Endpoint;

public static class UserProfileEndpoints
{
    public static RouteGroupBuilder MapUserProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profiles").WithTags("User Profiles").AddEndpointFilter<BusinessEndpointFilter>();
        group.MapGet("", SearchAsync);
        group.MapGet("/me", GetMineAsync).RequireAuthorization();
        group.MapPut("/me", SaveMineAsync).RequireAuthorization();
        group.MapGet("/{userId:guid}", GetByUserIdAsync);
        group.MapGet("/{userId:guid}/posts", GetPostsAsync);
        return group;
    }
    private static async Task<IResult> SearchAsync(UserProfileService service, CancellationToken ct, string? query = null, int pageNumber = 1, int pageSize = 20) =>
        Results.Ok(await service.SearchAsync(query, pageNumber, pageSize, ct));
    private static async Task<IResult> GetMineAsync(ClaimsPrincipal user, UserProfileService service, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        return await GetByUserIdAsync(me, service, ct);
    }
    private static async Task<IResult> GetByUserIdAsync(Guid userId, UserProfileService service, CancellationToken ct)
    {
        var profile = await service.GetByUserIdAsync(userId, ct);
        return profile is null ? BusinessResults.NotFound() : Results.Ok(profile);
    }
    private static async Task<IResult> GetPostsAsync(Guid userId, UserProfileService profiles, PostService posts, ClaimsPrincipal user, CancellationToken ct, int pageNumber = 1, int pageSize = 10)
    {
        if (await profiles.GetByUserIdAsync(userId, ct) is null) return BusinessResults.NotFound();
        return Results.Ok(await posts.GetPageAsync(pageNumber, pageSize, user.TryGetCurrentUserId(out var me) ? me : null, userId, ct));
    }
    private static async Task<IResult> SaveMineAsync(SaveUserProfileRequest request, ClaimsPrincipal user, UserProfileService service, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        var profile = await service.SaveAsync(me, request.DisplayName, request.Bio, ct);
        return profile is null ? Results.Unauthorized() : Results.Ok(profile);
    }
}
