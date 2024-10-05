namespace HolyricsCompanion.Storage;

public record HistoryItem
{
    public int Id { get; set; }
    public string HolyricsId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string Title { get; init; }
    public bool Sent { get; set; }
}