namespace SongList.Web.Auth;

public class TgOptions
{
    public string Token { get; init; }
    public long ChatId { get; init; }
    public int UpdatesThreadId { get; init; } = 722;
}