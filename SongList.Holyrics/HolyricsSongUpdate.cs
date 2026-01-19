namespace SongList.Holyrics;

public sealed class HolyricsSongUpdate
{
    public string? SyncId { get; set; }

    public long Id { get; set; }

    public long ModifiedTime { get; set; }

    public string? Title { get; set; }

    public string? Artist { get; set; }

    public string? Lyrics { get; set; }

    public string? Author { get; set; }

    public string? Note { get; set; }
}
