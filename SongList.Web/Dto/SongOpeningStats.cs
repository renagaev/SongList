using System.ComponentModel.DataAnnotations;

namespace SongList.Web.Dto;

public class SongOpeningStats
{
    [Required] public int Id { get; set; }

    [Required] public int Count { get; set; }
}