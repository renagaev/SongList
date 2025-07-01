namespace SongList.Web.Entities;

public class VerseHistoryItem
{
    public int Id { get; init; }
    public int Book { get; init; }
    public int Chapter { get; init; }
    public int Verse { get; init; }
    public DateTimeOffset ShowedAt { get; init; }
    public DateTimeOffset HiddenAt { get; init; }
}