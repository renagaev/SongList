namespace SongList.Holyrics;

internal sealed class HolyricsDriveFile
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string MimeType { get; set; } = "";
    public DateTimeOffset? ModifiedAt { get; set; }
    public long? Size { get; set; }
    public IDictionary<string, string> AppProperties { get; set; } = new Dictionary<string, string>();
}