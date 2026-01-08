using System.ComponentModel.DataAnnotations;

namespace SongList.Web.Entities;

public class Song
{
    public int Id { get; init; }
    public string Title { get; set; }
    public string[] Tags { get; set; } = [];
    public string Text { get; set; }
    public int? Number { get; set; }
    public string OriginalTitle { get; init; }

    public List<SongHistoryItem> History { get; init; } = [];
    public List<HolyricsSong> HolyricsSongs { get; init; } = [];
    public List<SongAttachment> Attachments { get; init; } = [];
    public Note? Note { get; init; }
    public int? NoteId { get; set; }
    public bool IsDeleted { get; set; }
}

public class SongOpeningStats
{
    [Required] public int Id { get; set; }

    [Required] public int Count { get; set; }
}