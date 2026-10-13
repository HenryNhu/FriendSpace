using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Social.Infrastructure.Identity;
using System.Security.Claims;

namespace Social.Api.Endpoint
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(
        this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth")
                .WithTags("Auth");

            group.MapIdentityApi<ApplicationUser>();

            group.MapGet("/session", GetSessionAsync);

            group.MapGet("/me", GetMeAsync)
                .RequireAuthorization();

            group.MapPost("/logout", LogoutAsync)
                .RequireAuthorization();
        }

        private static async Task<IResult> GetSessionAsync(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            IAntiforgery antiforgery)
        {
            context.Response.Headers["Cache-Control"] = "no-store";

            var account = await userManager.GetUserAsync(context.User);
            var tokens = antiforgery.GetAndStoreTokens(context);

            return Results.Ok(new
            {
                user = account is null
                    ? null
                    : new
                    {
                        id = account.Id,
                        email = account.Email
                    },

                csrfToken = tokens.RequestToken
                    ?? throw new InvalidOperationException(
                        "Không tạo được CSRF token.")
            });
        }

        private static async Task<IResult> GetMeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            context.Response.Headers["Cache-Control"] = "no-store";

            var account = await userManager.GetUserAsync(context.User);

            if (account is null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new
            {
                id = account.Id,
                email = account.Email
            });
        }

        private static async Task<IResult> LogoutAsync(SignInManager<ApplicationUser> signInManager)
        {
            await signInManager.SignOutAsync();

            return Results.NoContent();
        }
    }
}
