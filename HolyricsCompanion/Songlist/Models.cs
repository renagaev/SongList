namespace HolyricsCompanion.Songlist;

public class SongHistoryItem
{
    public string HolyricsId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string Title { get; init; }
}

public record SongSlideItem
{
    public string HolyricsId { get; init; }
    public int SlideNumber { get; init; }
    public int TotalSlides { get; init; }
    public string SongName { get; init; }
    public DateTimeOffset ShowedAt { get; init; }
    public DateTimeOffset HiddenAt { get; init; }
}

public record BibleVerseItem
{
    public int Book { get; init; }
    public int Chapter { get; init; }
    public int Verse { get; init; }
    public DateTimeOffset ShowedAt { get; init; }
    public DateTimeOffset HiddenAt { get; init; }
}