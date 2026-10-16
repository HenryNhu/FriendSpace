using System.Security.Claims;
using Social.Api.Authentication;
using Social.Application.Feed;

namespace Social.Api.Endpoint;

public static class FeedEndpoints
{
    public static RouteGroupBuilder MapFeedEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/feed").WithTags("Feed")
            .AddEndpointFilter<BusinessEndpointFilter>();
        group.MapGet("", GetPageAsync);
        group.MapGet("/updates", GetUpdatesAsync);
        return group;
    }

    private static async Task<IResult> GetPageAsync(
        FeedService service, ClaimsPrincipal user, CancellationToken ct,
        string? cursor = null, int pageSize = 10) =>
        Results.Ok(await service.GetPageAsync(
            user.TryGetCurrentUserId(out var me) ? me : null, cursor, pageSize, ct));

    private static async Task<IResult> GetUpdatesAsync(
        FeedService service, ClaimsPrincipal user, CancellationToken ct,
        string? after = null) =>
        Results.Ok(await service.GetUpdatesAsync(
            user.TryGetCurrentUserId(out var me) ? me : null, after, ct));
}
