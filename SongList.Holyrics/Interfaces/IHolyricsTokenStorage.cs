namespace SongList.Holyrics.Interfaces;

public interface IHolyricsTokenStorage
{
    public Task<HolyricsAuthToken?> GetTokenAsync(CancellationToken cancellationToken);
    public Task SaveTokenAsync(HolyricsAuthToken token, CancellationToken cancellationToken);
}