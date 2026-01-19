using System.Text.Json.Serialization;

namespace SongList.Holyrics;

internal sealed class HolyricsDriveFile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = "";

    [JsonPropertyName("modifiedTime")] 
    public DateTime? ModifiedTime { get; set; }

    [JsonPropertyName("modifiedByMeTime")]
    public DateTime? ModifiedByMeTime { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("md5Checksum")]
    public string Md5Checksum { get; set; } = "";

    [JsonPropertyName("appProperties")]
    public IDictionary<string, string> AppProperties { get; set; } = new Dictionary<string, string>();

    [JsonIgnore]
    public string CustomMd5 =>
        AppProperties != null && AppProperties.TryGetValue("custom_md5", out var value) ? value : "";
}