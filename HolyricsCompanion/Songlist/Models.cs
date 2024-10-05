namespace HolyricsCompanion.Songlist;

public class HistoryItem
{
    public string HolyricsId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string Title { get; init; }
}