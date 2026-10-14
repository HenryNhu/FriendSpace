using Microsoft.AspNetCore.Http.Features;
using Social.Application.Abstractions;
using Social.Application.Media;
using Social.Infrastructure.Media;

namespace Social.Api.Media;

public static class MediaServiceRegistration
{
    public const long MaxRequestBytes = MediaService.MaxFileBytes + 1024 * 1024;

    public static IServiceCollection AddFriendSpaceMedia(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = MaxRequestBytes;
            options.ValueLengthLimit = 4096;
            options.ValueCountLimit = 16;
        });
        services.AddSingleton<IMediaStorage>(provider =>
        {
            var environment = provider.GetRequiredService<IHostEnvironment>();
            return new AzureBlobMediaStorage(configuration["Media:ServiceUri"],
                configuration["Media:ContainerName"] ?? "friendspace-media",
                environment.IsDevelopment() ? configuration["Media:ConnectionString"] : null);
        });
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<MediaService>();
        services.AddHostedService<MediaCleanupService>();
        return services;
    }
}
