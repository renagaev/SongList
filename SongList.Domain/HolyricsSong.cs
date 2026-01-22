namespace SongList.Domain;

public class HolyricsSong
{
    public int Id { get; init; }
    public string HolyricsId { get; init; }
    public int? SongId { get; init; }
    public Song Song { get; set; }
    public string Title { get; set; }
    public string? Lyrics { get; set; }
    public string? Formatting { get; set; }
}