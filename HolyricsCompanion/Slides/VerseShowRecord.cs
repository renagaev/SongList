namespace HolyricsCompanion.Slides;

public record VerseShowRecord
{
    public int Id { get; set; }
    public int Book { get; set; }
    public int Chapter { get; set; }
    public int Verse { get; set; }
    public DateTimeOffset ShowedAt { get; set; }
    public DateTimeOffset HiddenAt { get; set; }
}