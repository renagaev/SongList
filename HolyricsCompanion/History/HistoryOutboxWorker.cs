using HolyricsCompanion.Songlist;
using HolyricsCompanion.Workers;
using Microsoft.Extensions.Options;

namespace HolyricsCompanion.History;

public class HistoryOutboxWorker(IOptionsMonitor<WorkersSettings> optionsMonitor, IServiceScopeFactory scopeFactory, ILogger<HistoryOutboxWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(optionsMonitor.CurrentValue.OutboxInterval, stoppingToken);

            using var scope = scopeFactory.CreateScope();
            var storage = scope.ServiceProvider.GetRequiredService<HistoryRepository>();
            var client = scope.ServiceProvider.GetRequiredService<SonglistClient>();

            var itemsToSend = storage.GetNonSentItems();
            logger.LogInformation($"found {itemsToSend.Length} new history items to send");
            foreach (var historyItem in itemsToSend)
            {
                try
                {
                    var dto = new SongHistoryItem()
                    {
                        HolyricsId = historyItem.HolyricsId,
                        CreatedAt = historyItem.CreatedAt,
                        Title = historyItem.Title
                    };
                    await client.ReportHistoryItem(dto, stoppingToken);
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