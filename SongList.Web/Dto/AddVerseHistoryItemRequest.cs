namespace SongList.Web.Dto;

public class AddVerseHistoryItemRequest
{
    public int Book { get; init; }
    public int Chapter { get; init; }
    public int Verse { get; init; }
    public DateTimeOffset ShowedAt { get; init; }
    public DateTimeOffset HiddenAt { get; init; }
}