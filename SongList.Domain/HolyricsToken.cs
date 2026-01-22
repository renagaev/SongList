namespace SongList.Domain;

public class HolyricsToken
{
    public int Id { get; init; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public long ExpiresIn { get; set; }
    public long CreatedAt { get; set; }
}