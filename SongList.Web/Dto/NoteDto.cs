using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SongList.Web.Dto;

[DisplayName("Note")]
public record NoteDto
{
    public int Id { get; init; }
    [Required]
    public string Name { get; init; }
    [Required]
    public string SimpleName { get; init; }
    
    [Required]
    public string DetailedName { get; init; }
}