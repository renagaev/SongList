namespace SongList.Web.Dto;

public class RecomputePredictionsRequest
{
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
}