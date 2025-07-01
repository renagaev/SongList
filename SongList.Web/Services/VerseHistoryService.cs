using Microsoft.EntityFrameworkCore;
using SongList.Web.Dto;
using SongList.Web.Entities;

namespace SongList.Web.Services;

public class VerseHistoryService(AppContext context)
{
    public async Task AddHistoryItem(AddVerseHistoryItemRequest item, CancellationToken cancellationToken)
    {
        var exists = await context.VerseHistory.AnyAsync(x => x.ShowedAt == item.ShowedAt, cancellationToken);
        if (exists)
        {
            return;
        }

        context.VerseHistory.Add(new VerseHistoryItem
        {
            Book = item.Book,
            Chapter = item.Chapter,
            Verse = item.Verse,
            ShowedAt = item.ShowedAt,
            HiddenAt = item.HiddenAt
        });
        await context.SaveChangesAsync(cancellationToken);
    }
}