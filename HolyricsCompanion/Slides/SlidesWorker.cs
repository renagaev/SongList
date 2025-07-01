using HolyricsCompanion.Holyrics;
using HolyricsCompanion.Workers;
using Microsoft.Extensions.Options;

namespace HolyricsCompanion.Slides;

public class SlidesWorker(
    IOptionsMonitor<WorkersSettings> optionsMonitor,
    IServiceScopeFactory scopeFactory,
    VerseRepository verseRepository,
    SongSlideRepository songSlideRepository,
    ILogger<SlidesWorker> logger) : BackgroundService
{
    private SongSlideShowRecord? _currentSongSlide;
    private VerseShowRecord? _currentVerse;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(optionsMonitor.CurrentValue.SlidePollingInterval, stoppingToken);
            using var scope = scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<HolyricsClient>();
            try
            {
                var currentSlide = await client.GetCurrentSlide(stoppingToken);
                var now = DateTimeOffset.UtcNow;
                if (currentSlide == null)
                {
                    FlushCurrentVerse(now);
                    FlushCurrentSongSlide(now);
                }
                else
                    switch (currentSlide.Type)
                    {
                        case SlideType.Bible:
                        {
                            var verse = currentSlide.Verse!;
                            if (_currentVerse == null || _currentVerse.Book != verse.Book ||
                                _currentVerse.Chapter != verse.Chapter || _currentVerse.Verse != verse.Verse)
                            {
                                FlushCurrentVerse(now);
                                _currentVerse = new VerseShowRecord
                                {
                                    Book = verse.Book,
                                    Chapter = verse.Chapter,
                                    Verse = verse.Verse,
                                    ShowedAt = DateTimeOffset.UtcNow
                                };
                            }

                            FlushCurrentSongSlide(now);
                            break;
                        }
                        case SlideType.Song:
                        {
                            var song = currentSlide.SongSlide!;
                            if (_currentSongSlide == null || _currentSongSlide.HolyricsId != song.HolyricsId ||
                                _currentSongSlide.SlideNumber != song.SlideNumber)
                            {
                                FlushCurrentSongSlide(now);
                                _currentSongSlide = new SongSlideShowRecord
                                {
                                    HolyricsId = song.HolyricsId,
                                    SlideNumber = song.SlideNumber,
                                    TotalSlides = song.TotalSlides,
                                    SongName = song.SongName,
                                    ShowedAt = now
                                };
                            }

                            FlushCurrentVerse(now);
                            break;
                        }
                    }
            }
            catch (Exception e)
            {
                logger.LogError(e, "failed to get current slide");
            }
        }
    }

    private void FlushCurrentVerse(DateTimeOffset now)
    {
        if (_currentVerse == null)
            return;
        _currentVerse.HiddenAt = now;
        verseRepository.Upsert(_currentVerse);
        _currentVerse = null;
    }

    private void FlushCurrentSongSlide(DateTimeOffset now)
    {
        if (_currentSongSlide == null)
            return;
        _currentSongSlide.HiddenAt = now;
        songSlideRepository.Upsert(_currentSongSlide);
        _currentSongSlide = null;
    }
}