using Microsoft.EntityFrameworkCore;
using SongList.Holyrics;
using SongList.Holyrics.Interfaces;

namespace SongList.Web.Services;

public class HolyricsTokenStorage(IServiceScopeFactory factory): IHolyricsTokenStorage
{
    public async Task<HolyricsAuthToken?> GetTokenAsync(CancellationToken cancellationToken)
    {
        using var scope = factory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppContext>();
        var entity = await context.HolyricsToken.FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
        {
            return null;
        }

        return new HolyricsAuthToken
        {
            AccessToken = entity.AccessToken,
            RefreshToken = entity.RefreshToken,
            CreatedAt = entity.CreatedAt,
            ExpiresIn = entity.ExpiresIn
        };
    }

    public async Task SaveTokenAsync(HolyricsAuthToken token, CancellationToken cancellationToken)
    {
        using var scope = factory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppContext>();
        var entity = await context.HolyricsToken.FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
        {
            entity = new Domain.HolyricsToken();
            context.HolyricsToken.Add(entity);
        }
        entity.AccessToken = token.AccessToken;
        entity.RefreshToken = token.RefreshToken;
        entity.CreatedAt = token.CreatedAt;
        entity.ExpiresIn = token.ExpiresIn;
        await context.SaveChangesAsync(cancellationToken);
    }
}