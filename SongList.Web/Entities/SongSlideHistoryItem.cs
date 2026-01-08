namespace SongList.Web.Entities;

public class SongSlideHistoryItem
{
    public int Id { get; set; }
    public int SlideNumber { get; set; }
    public int TotalSlides { get; set; }
    public HolyricsSong HolyricsSong { get; set; }
    public int HolyricsSongId { get; set; }
    public int? SongShowId { get; set; }
    public DateTimeOffset ShowedAt { get; set; }
    public DateTimeOffset HiddenAt { get; set; }
}