using HolyricsCompanion.Holyrics;
using HolyricsCompanion.Workers;
using Microsoft.Extensions.Options;

namespace HolyricsCompanion.History;

public class HolyricsWorker(IOptionsMonitor<WorkersSettings> optionsMonitor, IServiceScopeFactory scopeFactory, ILogger<HolyricsWorker> logger): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(optionsMonitor.CurrentValue.HistoryPollingInterval, stoppingToken);
            using var scope = scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<HolyricsClient>();
            var storage = scope.ServiceProvider.GetRequiredService<HistoryRepository>();
            
            try
            {
                var historyItems = await client.GetHistories(stoppingToken);
                logger.LogInformation($"received {historyItems.Length} history items");
                foreach (var historyItem in historyItems)
                {
                    var recordedLastTime = storage.GetSongLastTime(historyItem.MusicId);
                    var newTimes = historyItem.History.Where(x => x > recordedLastTime).ToList();
                    if (newTimes.Count < 0)
                    {
                        continue;
                    }
                    var title = await client.GetTitle(historyItem.MusicId, stoppingToken);
                    foreach (var createdAt in newTimes)
                    {
                        var item = new HistoryItem
                        {
                            CreatedAt = createdAt,
                            Title = title,
                            Sent = false,
                            HolyricsId = historyItem.MusicId
                        };
                        storage.Upsert(item);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogInformation(e, "failed to receive new history items");
            }
        }
    }
}