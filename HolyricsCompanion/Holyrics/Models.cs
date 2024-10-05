using System.Text.Json.Serialization;

namespace HolyricsCompanion.Holyrics;

public record SongHistory
{
    [JsonPropertyName("music_id")]
    public string MusicId { get; init; }
    public DateTimeOffset[] History { get; init; }
}

public record TitleRequest
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
}

public record TitleResponse
{
    [JsonPropertyName("title")]
    public string Title { get; init; }
}


public record ResponseWrapper<T>
{
    public T Data { get; init; }
}