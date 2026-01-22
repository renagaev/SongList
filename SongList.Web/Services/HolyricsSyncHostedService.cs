using SongList.Web.UseCases.SyncHolyricsSongs;

namespace SongList.Web.Services;

public class HolyricsSyncHostedService(IServiceScopeFactory scopeFactory, ILogger<HolyricsSyncHostedService> logger): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<SyncHolyricsSongsHandler>();
                await handler.Handle(stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error during holyrics sync");
            }

        }
    }
}