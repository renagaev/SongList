namespace SongList.Web.Dto;

public record SongLastHistoryDto
{
    public int SongId { get; init; }
    public DateTimeOffset? LastMorning { get; init; }
    public DateTimeOffset? LastEvening { get; init; }
}
