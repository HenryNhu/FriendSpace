using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace Social.Api.Authentication
{
    public static class BrowserSecurityExtensions
    {
        public static IServiceCollection AddBrowserSecurity(this IServiceCollection services)
        {
            services.AddDataProtection().SetApplicationName("FriendSpace");

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "FriendSpace.Auth";
                options.Cookie.Path = "/api";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;

                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;

                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;

                    return Task.CompletedTask;
                };
            });

            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";

                options.Cookie.Name = "FriendSpace.Antiforgery";
                options.Cookie.Path = "/api";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            return services;
        }

        public static IApplicationBuilder UseBrowserCsrf(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var isReadRequest = HttpMethods.IsGet(context.Request.Method) ||
                                    HttpMethods.IsHead(context.Request.Method) ||
                                    HttpMethods.IsOptions(context.Request.Method);

                if (context.Request.Path.StartsWithSegments("/api") && !isReadRequest)
                {
                    var bearer = await context.AuthenticateAsync(
                        IdentityConstants.BearerScheme);

                    if (!bearer.Succeeded)
                    {
                        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();

                        try
                        {
                            await antiforgery.ValidateRequestAsync(context);
                        }
                        catch (AntiforgeryValidationException)
                        {
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;

                            await context.Response.WriteAsJsonAsync(new
                            {
                                code = "invalid_csrf",
                                message = "Phiên đã thay đổi. Hãy tải lại trang và thử lại."
                            });

                            return;
                        }
                    }
                }

                await next(context);
            });
        }
    }
}
