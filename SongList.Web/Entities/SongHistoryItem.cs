namespace SongList.Web.Entities;

public class SongHistoryItem
{
    public int Id { get; init; }
    public Song Song { get; init; }
    public int? SongId { get; init; }
    public string HolyricsId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}