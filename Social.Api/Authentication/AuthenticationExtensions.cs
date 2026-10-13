using Social.Infrastructure.Identity;
using Social.Infrastructure.Persistence;

namespace Social.Api.Authentication
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddFriendSpaceAuthentication(this IServiceCollection services)
        {
            services
                .AddIdentityApiEndpoints<ApplicationUser>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequiredLength = 8;
                })
                .AddEntityFrameworkStores<SocialDbContext>();

            services.AddBrowserSecurity();

            services.AddAuthorization();

            return services;
        }
    }
}
