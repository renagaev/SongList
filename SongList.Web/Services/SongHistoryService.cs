using Microsoft.EntityFrameworkCore;
using SongList.Web.Dto;
using SongList.Web.Entities;

namespace SongList.Web.Services;

public class SongHistoryService(AppContext context)
{
    public async Task AddHistoryItem(string holyricsId, DateTimeOffset createdAt, string title, CancellationToken cancellationToken)
    {
        var holyricsSong = await context.HolyricsSongs.FirstOrDefaultAsync(x => x.HolyricsId == holyricsId, cancellationToken);
        if (holyricsSong != null)
        {
            var itemExists = await context.History.AnyAsync(x => x.HolyricsSong.Id == holyricsSong.Id && x.CreatedAt == createdAt, cancellationToken);
            if (itemExists)
                return;
        }
        if (holyricsSong == null)
        {
            holyricsSong = new HolyricsSong
            {
                HolyricsId = holyricsId,
                Title = title
            };
            context.HolyricsSongs.Add(holyricsSong);
        }

        context.History.Add(new SongHistoryItem
        {
            HolyricsSong = holyricsSong,
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
            .Where(x => x.HolyricsSong.SongId != null)
            .GroupBy(x => x.HolyricsSong.SongId!.Value)
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
        return [];
    }

    public async Task AddSlideHistoryItem(AddSlideHistoryItemRequest request, CancellationToken cancellationToken)
    {
        
        var holyricsSong = await context.HolyricsSongs.FirstOrDefaultAsync(x => x.HolyricsId == request.HolyricsId, cancellationToken);
        if (holyricsSong != null)
        {
            var itemExists = await context.SlideHistory.AnyAsync(x => x.HolyricsSong.Id == holyricsSong.Id && x.ShowedAt == request.ShowedAt, cancellationToken);
            if (itemExists)
                return;
        }
        if (holyricsSong == null)
        {
            holyricsSong = new HolyricsSong
            {
                HolyricsId = request.HolyricsId,
                Title = request.SongName
            };
            context.HolyricsSongs.Add(holyricsSong);
        }


        context.SlideHistory.Add(new SongSlideHistoryItem
        {
            HolyricsSong = holyricsSong,
            SlideNumber = request.SlideNumber,
            TotalSlides = request.TotalSlides,
            ShowedAt = request.ShowedAt,
            HiddenAt = request.HiddenAt
        });
        await context.SaveChangesAsync(cancellationToken);
    }
}
