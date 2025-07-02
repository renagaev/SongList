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

public record CurrentSlideResponse
{
    [JsonPropertyName("type")]
    public string Type { get; init; }
    
    [JsonPropertyName("name")]
    public string SongName { get; init; }
    [JsonPropertyName("slide_number")]
    public int? SlideNumber { get; init; }
    [JsonPropertyName("total_slides")]
    public int? TotalSlides { get; init; }
    [JsonPropertyName("song_id")]
    public string SongId { get; init; }
    
    
    [JsonPropertyName("id")]
    public string Id { get; init; }
}

public record ResponseWrapper<T>
{
    public T? Data { get; init; }
}