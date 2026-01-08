namespace SongList.Web.Entities;

public class HolyricsSong
{
    public int Id { get; init; }
    public string HolyricsId { get; init; }
    public int? SongId { get; init; }
    public Song Song { get; init; }
    public string Title { get; init; }
}