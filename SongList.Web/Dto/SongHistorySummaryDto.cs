namespace SongList.Web.Dto;

public record SongHistorySummaryDto
{
    public int SongId { get; init; }
    public DateTimeOffset? LastMorning { get; init; }
    public DateTimeOffset? LastEvening { get; init; }
    public DateTimeOffset? Last { get; init; }
}
