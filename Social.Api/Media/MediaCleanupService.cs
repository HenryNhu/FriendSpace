using Social.Application.Abstractions;

namespace Social.Api.Media;

public sealed class MediaCleanupService(IServiceScopeFactory scopeFactory,
    ILogger<MediaCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Do not query while a new database is still being migrated during startup.
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IMediaRepository>();
                    await repository.PurgeExpiredAsync(DateTimeOffset.UtcNow.AddHours(-24), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception exception)
                {
                    // Failed blobs retain their rows and are retried at the next tick.
                    logger.LogWarning(exception, "Could not complete expired media cleanup; it will be retried.");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }
}
