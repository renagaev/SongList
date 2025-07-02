namespace SongList.Web.Dto;

public class AddSlideHistoryItemRequest
{
    public string HolyricsId { get; init; }
    public int SlideNumber { get; init; }
    public int TotalSlides { get; init; }
    public string SongName { get; init; }
    public DateTimeOffset ShowedAt { get; init; }
    public DateTimeOffset HiddenAt { get; init; }
}