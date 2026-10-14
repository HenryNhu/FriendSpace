using Social.Application.Common;
namespace Social.Api.Endpoint;

public sealed class BusinessEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try { return await next(context); }
        catch (InvalidMediaException ex) { return Results.BadRequest(new { code = "invalid_media", message = ex.Message }); }
        catch (ArgumentException ex) { return Results.BadRequest(new { code = "validation_error", message = ex.Message }); }
        catch (FriendshipExistsException ex) { return Results.Conflict(new { code = "friendship_exists", message = ex.Message }); }
    }
}
public static class BusinessResults
{
    public static IResult NotFound() => Results.NotFound(new { code = "not_found", message = "Không tìm thấy nội dung hoặc bạn không có quyền truy cập." });
}
