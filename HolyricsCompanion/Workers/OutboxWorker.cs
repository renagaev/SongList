using HolyricsCompanion.Songlist;
using HolyricsCompanion.Storage;
using Microsoft.Extensions.Options;

namespace HolyricsCompanion.Workers;

public class OutboxWorker(IOptionsMonitor<WorkersSettings> optionsMonitor, IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(optionsMonitor.CurrentValue.OutboxInterval, stoppingToken);

            using var scope = scopeFactory.CreateScope();
            var storage = scope.ServiceProvider.GetRequiredService<Repository>();
            var client = scope.ServiceProvider.GetRequiredService<SonglistClient>();

            var itemsToSend = storage.GetNonSentItems();
            foreach (var historyItem in itemsToSend)
            {
                try
                {
                    await client.ReportItem(historyItem.HolyricsId, historyItem.CreatedAt, historyItem.Title,
                        stoppingToken);
                    historyItem.Sent = true;
                    storage.Upsert(historyItem);
                }
                catch (Exception e)
                {
                    // ignored
                }
            }


        }
    }
}