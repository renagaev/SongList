using System.Text.Json.Serialization;

namespace SongList.Holyrics;


public sealed class HolyricsDeletedItemsPayload
{
    [JsonPropertyName("deleted")]
    public List<HolyricsDeletedItem> Deleted { get; set; } = new();
}

public sealed class HolyricsSyncSong
{
    [JsonPropertyName("syncId")]
    public string SyncId { get; set; } = "";

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("artist")]
    public string Artist { get; set; } = "";

    [JsonPropertyName("lyrics")]
    public string Lyrics { get; set; } = "";

    [JsonPropertyName("order")]
    public List<string>? Order { get; set; }

    [JsonPropertyName("author")]
    public string Author { get; set; } = "";

    [JsonPropertyName("note")]
    public string Note { get; set; } = "";

    [JsonPropertyName("language")]
    public string Language { get; set; } = "";

    [JsonPropertyName("formatting")]
    public string Formatting { get; set; } = "";

    [JsonPropertyName("set")]
    public long Set { get; set; }

    [JsonPropertyName("titleToSearch")]
    public string? TitleToSearch { get; set; }

    [JsonPropertyName("artistToSearch")]
    public string? ArtistToSearch { get; set; }

    [JsonPropertyName("lyricsToSearch")]
    public string? LyricsToSearch { get; set; }

    [JsonPropertyName("noteToSearch")]
    public string? NoteToSearch { get; set; }

    [JsonPropertyName("lyricsHTML")]
    public string? LyricsHtml { get; set; }

    [JsonPropertyName("titleHTML")]
    public string? TitleHtml { get; set; }

    [JsonPropertyName("artistHTML")]
    public string? ArtistHtml { get; set; }

    [JsonPropertyName("src")]
    public string Src { get; set; } = "";

    [JsonPropertyName("numIni")]
    public int? NumIni { get; set; }

    [JsonPropertyName("translations")]
    public string[][]? Translations { get; set; }

    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    [JsonPropertyName("modifiedTime")]
    public long ModifiedTime { get; set; }

    [JsonPropertyName("itemModifiedTime")]
    public long ItemModifiedTime { get; set; }

    [JsonPropertyName("midi")]
    public string? Midi { get; set; }

    [JsonPropertyName("strParams")]
    public string[]? StrParams { get; set; }

    [JsonPropertyName("version")]
    public long Version { get; set; }

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }
}

public sealed class HolyricsDeletedItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("modifiedTime")]
    public long ModifiedTime { get; set; }
}
