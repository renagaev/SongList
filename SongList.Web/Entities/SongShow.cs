namespace SongList.Web.Entities;

public class SongShow
{
    public int Id { get; init; }
    public List<SongSlideHistoryItem> Slides { get; init; }
    public HolyricsSong HolyricsSong { get; init; }
    public int HolyricsSongId { get; init; }
    public DateTimeOffset ShowedAt { get; set; }
    public DateTimeOffset HiddenAt { get; set; }

    public float? Score { get; set; }
    public bool? IsServiceModel { get; set; }
    public bool? IsServiceFix { get; set; }

    public bool? IsServiceTrain { get; set; }
}