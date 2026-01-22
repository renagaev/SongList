using System.Text.Json.Serialization;

namespace SongList.Holyrics;

public sealed class HolyricsAuthToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";
    
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    public bool IsExpired(DateTimeOffset? now = null)
    {
        var current = now ?? DateTimeOffset.UtcNow;
        var currentSeconds = current.ToUnixTimeSeconds();
        return CreatedAt + ExpiresIn <= currentSeconds;
    }
}
