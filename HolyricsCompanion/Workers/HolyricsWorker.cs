using HolyricsCompanion.Holyrics;
using HolyricsCompanion.Storage;
using Microsoft.Extensions.Options;

namespace HolyricsCompanion.Workers;

public class HolyricsWorker(IOptionsMonitor<WorkersSettings> optionsMonitor, IServiceScopeFactory scopeFactory): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(optionsMonitor.CurrentValue.HistoryPollingInterval, stoppingToken);
            using var scope = scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<HolyricsClient>();
            var storage = scope.ServiceProvider.GetRequiredService<Repository>();
            
            try
            {
                var historyItems = await client.GetHistories(stoppingToken);
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
                var x = 0;
                // ignored
            }
        }
    }
}