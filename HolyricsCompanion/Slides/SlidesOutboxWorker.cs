using HolyricsCompanion.Songlist;
using HolyricsCompanion.Workers;
using Microsoft.Extensions.Options;

namespace HolyricsCompanion.Slides;

public class SlidesOutboxWorker(IOptionsMonitor<WorkersSettings> optionsMonitor, IServiceScopeFactory scopeFactory, ILogger<SlidesOutboxWorker> logger): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(optionsMonitor.CurrentValue.OutboxInterval, stoppingToken);
            using var scope = scopeFactory.CreateScope();
            await FlushVerses(scope, stoppingToken);
            await FlushSongs(scope, stoppingToken);
        }
    }

    private async Task FlushVerses(IServiceScope scope, CancellationToken stoppingToken)
    {
        var repository = scope.ServiceProvider.GetRequiredService<VerseRepository>();
        var client = scope.ServiceProvider.GetRequiredService<SonglistClient>();
        var verses = repository.GetAll();
        foreach (var verse in verses)
        {
            try
            {
                var model = new BibleVerseItem
                {
                    Book = verse.Book,
                    Chapter = verse.Chapter,
                    Verse = verse.Verse,
                    ShowedAt = verse.ShowedAt,
                    HiddenAt = verse.HiddenAt
                };
                await client.ReportBibleVerse(model, stoppingToken);
                repository.Remove(verse);
            }
            catch(Exception e)
            {
                logger.LogError(e, "failed to report verse");
            }
        }
    }

    private async Task FlushSongs(IServiceScope scope, CancellationToken stoppingToken)
    {
        var repository = scope.ServiceProvider.GetRequiredService<SongSlideRepository>();
        var client = scope.ServiceProvider.GetRequiredService<SonglistClient>();
        var slides = repository.GetAll();
        foreach (var slide in slides)
        {
            try
            {
                var model = new SongSlideItem
                {
                    HolyricsId = slide.HolyricsId,
                    SlideNumber = slide.SlideNumber,
                    TotalSlides = slide.TotalSlides,
                    SongName = slide.SongName,
                    ShowedAt = slide.ShowedAt,
                    HiddenAt = slide.HiddenAt,
                };
                await client.ReportSongSlide(model, stoppingToken);
                repository.Remove(slide);
            }
            catch (Exception e)
            {
                logger.LogError(e, "failed to report song slide");
            }
        }
    }
}