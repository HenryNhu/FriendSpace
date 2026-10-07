using Microsoft.AspNetCore.Identity;
using Social.Infrastructure.Identity;
using System.Security.Claims;

namespace Social.Api.Endpoint
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth")
                .WithTags("Auth");

            group.MapIdentityApi<ApplicationUser>();

            group.MapGet("/me", GetMeAsync)
                .RequireAuthorization();
        }

        private static async Task<IResult> GetMeAsync(ClaimsPrincipal user,UserManager<ApplicationUser> userManager)
        {
            var account = await userManager.GetUserAsync(user);

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
    }
}
