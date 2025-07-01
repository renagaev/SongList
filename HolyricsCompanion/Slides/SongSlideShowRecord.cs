namespace HolyricsCompanion.Slides;

public record SongSlideShowRecord
{
    public int Id { get; set; }
    public string HolyricsId { get; set; }
    public string SongName { get; set; }
    public int SlideNumber { get; set; }
    public int TotalSlides { get; set; }
    public DateTimeOffset ShowedAt { get; set; }
    public DateTimeOffset HiddenAt { get; set; }
}