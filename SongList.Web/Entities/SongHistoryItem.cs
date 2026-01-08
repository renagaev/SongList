namespace SongList.Web.Entities;

public class SongHistoryItem
{
    public int Id { get; init; }
    public HolyricsSong HolyricsSong { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}