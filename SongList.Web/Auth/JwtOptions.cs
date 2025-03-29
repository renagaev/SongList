namespace SongList.Web.Auth;

public class JwtOptions
{
    public string Issuer { get; init; }
    public string Key { get; init; }
    public TimeSpan Expiration { get; init; } = TimeSpan.FromDays(30);
}