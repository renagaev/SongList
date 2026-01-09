using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SongList.Web.Dto;

[DisplayName("Song")]
public record SongDto
{
    [Required] public int Id { get; set; }
    [Required] public string Title { get; set; }
    [Required] public string[] Tags { get; set; } = [];
    [Required] public string Text { get; set; }
    public int? Number { get; set; }
    public string? Note { get; set; }
    public int? NoteId { get; set; }
};