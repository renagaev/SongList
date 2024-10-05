using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SongList.Web.Dto;
[DisplayName("Song")]
public record SongDto
{
    [Required]
    public int Id { get; init; }
    [Required]
    public string Title { get; init; }
    [Required]
    public string[] Tags { get; init; } = [];
    [Required]
    public string Text { get; init; }
    public int? Number { get; init; }
    public string? Note { get; init; }
};