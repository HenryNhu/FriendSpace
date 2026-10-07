using System.Security.Claims;

namespace Social.Api.Authentication
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool TryGetCurrentUserId(this ClaimsPrincipal user, out Guid userId)
        {
            userId = Guid.Empty;

            return user.Identity?.IsAuthenticated == true && 
                Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId) && 
                userId != Guid.Empty;
        }
    }
}
