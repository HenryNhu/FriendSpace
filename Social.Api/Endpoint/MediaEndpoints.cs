using Microsoft.AspNetCore.Http.Features;
using Social.Api.Authentication;
using Social.Api.Media;
using Social.Application.Media;
using System.Security.Claims;

namespace Social.Api.Endpoint;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media").WithTags("Media");
        group.MapPost("", UploadAsync).RequireAuthorization();
        group.MapGet("/{id:guid}", ReadAsync);
        group.MapDelete("/{id:guid}", DeleteAsync).RequireAuthorization();
        app.MapPut("/api/profiles/me/avatar", UploadAvatarAsync).RequireAuthorization().WithTags("Profiles");
        app.MapDelete("/api/profiles/me/avatar", RemoveAvatarAsync).RequireAuthorization().WithTags("Profiles");
        return app;
    }

    private static Task<IResult> UploadAsync(HttpContext context, MediaService service, CancellationToken cancellationToken)
        => UploadImageAsync(context, service, false, cancellationToken);

    private static Task<IResult> UploadAvatarAsync(HttpContext context, MediaService service, CancellationToken cancellationToken)
        => UploadImageAsync(context, service, true, cancellationToken);

    private static async Task<IResult> UploadImageAsync(HttpContext context, MediaService service, bool asAvatar,
        CancellationToken cancellationToken)
    {
        if (!context.User.TryGetCurrentUserId(out var ownerId)) return Results.Unauthorized();
        try
        {
            var request = context.Request;
            if (request.ContentLength > MediaServiceRegistration.MaxRequestBytes)
                throw new MediaException("image_too_large", "Mỗi ảnh tối đa 5 MB.", 413);
            var sizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
            if (sizeFeature is { IsReadOnly: false }) sizeFeature.MaxRequestBodySize = MediaServiceRegistration.MaxRequestBytes;
            if (!request.HasFormContentType || !request.ContentType!.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
                throw new MediaException("validation_error", "Hãy gửi ảnh trong trường file của multipart/form-data.");

            // Our existing UseBrowserCsrf middleware validates the X-CSRF-TOKEN
            // header. Manual parsing avoids a second, different antiforgery flow.
            var form = await request.ReadFormAsync(cancellationToken);
            var file = form.Files.GetFile("file");
            if (form.Files.Count != 1 || file is null)
                throw new MediaException("validation_error", "Mỗi lần tải lên cần đúng một ảnh trong trường file.");
            await using var stream = file.OpenReadStream();
            var response = await service.UploadAsync(ownerId, stream, file.Length, asAvatar, cancellationToken);
            return Results.Ok(response);
        }
        catch (MediaException exception) { return Error(exception); }
        catch (InvalidDataException)
        {
            return Results.Json(new { code = "validation_error", message = "Dữ liệu tải lên không hợp lệ hoặc vượt giới hạn 5 MB mỗi ảnh." }, statusCode: 400);
        }
        catch (BadHttpRequestException exception) when (exception.StatusCode == 413)
        {
            return Results.Json(new { code = "image_too_large", message = "Mỗi ảnh tối đa 5 MB." }, statusCode: 413);
        }
    }

    private static async Task<IResult> ReadAsync(Guid id, HttpContext context, MediaService service, CancellationToken cancellationToken)
    {
        Guid? viewerId = context.User.TryGetCurrentUserId(out var userId) ? userId : null;
        context.Response.Headers.CacheControl = "private, no-store";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        try
        {
            var media = await service.ReadAsync(id, viewerId, cancellationToken);
            if (media is null) return Results.NotFound();
            return Results.Stream(media.Content, media.ContentType, enableRangeProcessing: false);
        }
        catch (MediaException exception) { return Error(exception); }
    }

    private static async Task<IResult> DeleteAsync(Guid id, ClaimsPrincipal user, MediaService service, CancellationToken cancellationToken)
    {
        if (!user.TryGetCurrentUserId(out var ownerId)) return Results.Unauthorized();
        try
        {
            return await service.DeleteUnattachedAsync(id, ownerId, cancellationToken)
                ? Results.NoContent() : Results.NotFound();
        }
        catch (MediaException exception) { return Error(exception); }
    }

    private static async Task<IResult> RemoveAvatarAsync(ClaimsPrincipal user, MediaService service, CancellationToken cancellationToken)
    {
        if (!user.TryGetCurrentUserId(out var ownerId)) return Results.Unauthorized();
        await service.RemoveAvatarAsync(ownerId, cancellationToken);
        return Results.NoContent();
    }

    private static IResult Error(MediaException exception) => Results.Json(
        new { code = exception.Code, message = exception.Message }, statusCode: exception.StatusCode);
}
