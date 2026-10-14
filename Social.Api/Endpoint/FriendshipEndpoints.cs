using System.Security.Claims;
using Social.Api.Authentication;
using Social.Application.Friendships;
namespace Social.Api.Endpoint;

public record CreateFriendshipRequest(Guid UserId);
public static class FriendshipEndpoints
{
    public static RouteGroupBuilder MapFriendshipEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/friendships").WithTags("Friendships").RequireAuthorization().AddEndpointFilter<BusinessEndpointFilter>();
        group.MapGet("", GetMineAsync);
        group.MapGet("/with/{userId:guid}", GetWithAsync);
        group.MapPost("", CreateAsync);
        group.MapPost("/{id:guid}/accept", AcceptAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
        return group;
    }
    private static async Task<IResult> GetMineAsync(ClaimsPrincipal user, FriendshipService service, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        return Results.Ok(await service.GetMineAsync(me, ct));
    }
    private static async Task<IResult> GetWithAsync(Guid userId, ClaimsPrincipal user, FriendshipService service, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        var state = await service.GetWithAsync(me, userId, ct);
        return state is null ? BusinessResults.NotFound() : Results.Ok(state);
    }
    private static async Task<IResult> CreateAsync(CreateFriendshipRequest request, ClaimsPrincipal user, FriendshipService service, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        var row = await service.CreateAsync(me, request.UserId, ct);
        return row is null ? BusinessResults.NotFound() : Results.Ok(row);
    }
    private static async Task<IResult> AcceptAsync(Guid id, ClaimsPrincipal user, FriendshipService service, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        return await service.AcceptAsync(id, me, ct) ? Results.NoContent() : BusinessResults.NotFound();
    }
    private static async Task<IResult> DeleteAsync(Guid id, ClaimsPrincipal user, FriendshipService service, CancellationToken ct)
    {
        if (!user.TryGetCurrentUserId(out var me)) return Results.Unauthorized();
        return await service.DeleteAsync(id, me, ct) ? Results.NoContent() : BusinessResults.NotFound();
    }
}
