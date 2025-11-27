using Microsoft.EntityFrameworkCore;
using SongList.Web.Dto;
using SongList.Web.Entities;

namespace SongList.Web.Services;

public class SongHistoryService(AppContext context)
{
    public async Task AddHistoryItem(string holyricsId, DateTimeOffset createdAt, string title,
        CancellationToken cancellationToken)
    {
        var mapping = await context.Mappings.FirstOrDefaultAsync(x => x.HolyricsId == holyricsId, cancellationToken);
        if (mapping == null)
        {
            mapping = new SongMapping
            {
                HolyricsId = holyricsId,
                Title = title
            };
            context.Mappings.Add(mapping);
        }

        var itemExists = await (mapping.SongId switch
        {
            null => context.History.AnyAsync(x => x.HolyricsId == holyricsId && x.CreatedAt == createdAt,
                cancellationToken),
            { } songId => context.History.AnyAsync(x => x.SongId == songId && x.CreatedAt == createdAt,
                cancellationToken)
        });
        if (itemExists)
            return;

        context.History.Add(new SongHistoryItem
        {
            SongId = mapping.SongId,
            HolyricsId = holyricsId,
            CreatedAt = createdAt
        });
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<SongLastHistoryDto[]> GetLastSongHistory(CancellationToken cancellationToken)
    {
        var moscowBorderUtc = TimeSpan.FromHours(13);

        var songs = await context.Songs
            .Where(x => !x.IsDeleted)
            .Select(x => x.Id)
            .ToArrayAsync(cancellationToken);

        var history = await context.History
            .Where(x => x.SongId != null)
            .GroupBy(x => x.SongId!.Value)
            .Select(g => new
            {
                SongId = g.Key,
                LastMorning = g.Where(x => x.CreatedAt.TimeOfDay < moscowBorderUtc)
                    .Max(x => (DateTimeOffset?)x.CreatedAt),
                LastEvening = g.Where(x => x.CreatedAt.TimeOfDay >= moscowBorderUtc)
                    .Max(x => (DateTimeOffset?)x.CreatedAt)
            })
            .ToDictionaryAsync(x => x.SongId, cancellationToken);

        return songs
            .Select(id =>
            {
                var exists = history.TryGetValue(id, out var last);
                return new SongLastHistoryDto
                {
                    SongId = id,
                    LastMorning = exists ? last!.LastMorning : null,
                    LastEvening = exists ? last!.LastEvening : null
                };
            })
            .ToArray();
    }

    public async Task<ServiceDto[]> GetServices(CancellationToken cancellationToken)
    {
        var moscowBorderUtc = TimeSpan.FromHours(13);

        return await context.History
            .Where(x => x.SongId.HasValue)
            .GroupBy(x => new
                { x.CreatedAt.Date, IsMorning = x.CreatedAt.TimeOfDay < moscowBorderUtc })
            .Select(x => new ServiceDto
            {
                Date = DateOnly.FromDateTime(x.Key.Date),
                Type = x.Key.IsMorning ? ServiceType.Morning : ServiceType.Evening,
                Songs = x.Select(x => x.SongId!.Value).ToArray()
            }).ToArrayAsync(cancellationToken);
    }

    public async Task AddSlideHistoryItem(AddSlideHistoryItemRequest request, CancellationToken cancellationToken)
    {
        var exists = await context.SlideHistory
            .AnyAsync(x => x.HolyricsId == request.HolyricsId && x.ShowedAt == request.ShowedAt, cancellationToken);
        if (exists)
        {
            return;
        }

        context.SlideHistory.Add(new SongSlideHistoryItem
        {
            HolyricsId = request.HolyricsId,
            SlideNumber = request.SlideNumber,
            TotalSlides = request.TotalSlides,
            SongName = request.SongName,
            ShowedAt = request.ShowedAt,
            HiddenAt = request.HiddenAt
        });
        await context.SaveChangesAsync(cancellationToken);
    }
}
