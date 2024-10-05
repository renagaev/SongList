using System.ComponentModel.DataAnnotations;

namespace SongList.Web.Entities;


public class Song
{

    public int Id { get; init; }
    public string Title { get; init; }
    public string[] Tags { get; init; } = [];
    public string Text { get; init; }
    public int? Number { get; init; }
    public string? Note { get; init; }
    public string OriginalTitle { get; init; }

    public List<SongHistoryItem> History { get; init; } = [];
    public List<SongMapping> Mappings { get; init; } = [];
}

public class SongOpeningStats
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public int Count { get; set; }
}