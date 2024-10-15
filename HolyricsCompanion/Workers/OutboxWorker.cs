using HolyricsCompanion.Songlist;
using HolyricsCompanion.Storage;
using Microsoft.Extensions.Options;

namespace HolyricsCompanion.Workers;

public class OutboxWorker(IOptionsMonitor<WorkersSettings> optionsMonitor, IServiceScopeFactory scopeFactory, ILogger<OutboxWorker> logger)
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
            logger.LogInformation($"found {itemsToSend.Length} new history items to send");
            foreach (var historyItem in itemsToSend)
            {
                try
                {
                    await client.ReportItem(historyItem.HolyricsId, historyItem.CreatedAt, historyItem.Title, stoppingToken);
                    historyItem.Sent = true;
                    storage.Upsert(historyItem);
                    logger.LogInformation("reported new item to server");
                }
                catch (Exception e)
                {
                    logger.LogInformation(e, "failed to report history item");
                }
            }


        }
    }
}