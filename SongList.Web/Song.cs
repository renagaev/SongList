using System.ComponentModel.DataAnnotations;

namespace SongList.Web;

public class Song
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string[] Tags { get; set; } = Array.Empty<string>();
    [Required]
    public string Text { get; set; }
    public int? Number { get; set; }
    public string? Note { get; set; }
    public string OriginalTitle { get; set; }
}

public class SongOpeningStats
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public int Count { get; set; }
}