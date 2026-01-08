using Microsoft.EntityFrameworkCore;
using SongList.Web.Dto;
using SongList.Web.Entities;

namespace SongList.Web.Services;

public class SongHistoryService(AppContext context)
{
    public async Task AddHistoryItem(string holyricsId, DateTimeOffset createdAt, string title,
        CancellationToken cancellationToken)
    {
        var holyricsSong =
            await context.HolyricsSongs.FirstOrDefaultAsync(x => x.HolyricsId == holyricsId, cancellationToken);
        if (holyricsSong != null)
        {
            var itemExists =
                await context.History.AnyAsync(x => x.HolyricsSong.Id == holyricsSong.Id && x.CreatedAt == createdAt,
                    cancellationToken);
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
        var holyricsSong =
            await context.HolyricsSongs.FirstOrDefaultAsync(x => x.HolyricsId == request.HolyricsId, cancellationToken);
        if (holyricsSong != null)
        {
            var itemExists = await context.SlideHistory.AnyAsync(
                x => x.HolyricsSong.Id == holyricsSong.Id && x.ShowedAt == request.ShowedAt, cancellationToken);
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
            await context.SaveChangesAsync(cancellationToken);
        }

        var slideItem = new SongSlideHistoryItem
        {
            HolyricsSong = holyricsSong,
            SlideNumber = request.SlideNumber,
            TotalSlides = request.TotalSlides,
            ShowedAt = request.ShowedAt,
            HiddenAt = request.HiddenAt
        };

        context.SlideHistory.Add(slideItem);

        await CreateShows(slideItem, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateShows(SongSlideHistoryItem slideItem, CancellationToken cancellationToken)
    {
        var gap = TimeSpan.FromMinutes(5);

        var upperBorder = slideItem.HiddenAt.Add(gap * 2);
        var lowerBorder = slideItem.ShowedAt.Add(-gap * 2);

        var shows = await context.SongShows
            .Where(x => x.HiddenAt >= lowerBorder && x.ShowedAt <= upperBorder)
            .Include(songShow => songShow.Slides)
            .ToListAsync(cancellationToken);

        var target = shows
            .Where(x => x.HolyricsSongId == slideItem.HolyricsSongId)
            .Where(x => DistanceToInterval(x.ShowedAt, x.HiddenAt, slideItem.ShowedAt, slideItem.HiddenAt) < gap)
            .MinBy(x => DistanceToInterval(x.ShowedAt, x.HiddenAt, slideItem.ShowedAt, slideItem.HiddenAt));

        if (target == null)
        {
            target = new SongShow
            {
                HolyricsSongId = slideItem.HolyricsSongId,
                Slides = [],
                ShowedAt = slideItem.ShowedAt,
                HiddenAt = slideItem.HiddenAt
            };
            shows.Add(target);
            context.SongShows.Add(target);
        }

        target.Slides.Add(slideItem);
        target.ShowedAt = target.ShowedAt <= slideItem.ShowedAt ? target.ShowedAt : slideItem.ShowedAt;
        target.HiddenAt = target.HiddenAt >= slideItem.HiddenAt ? target.HiddenAt : slideItem.HiddenAt;

        
        shows = shows.OrderBy(x => x.ShowedAt).ToList();
        while (true)
        {
            var merged = false;
            for (var i = 0; i <= shows.Count - 2; i++)
            {
                var curr = shows[i];
                var next = shows[i + 1];
                if (curr.HolyricsSongId != next.HolyricsSongId)
                {
                    continue;
                }
                if (DistanceToInterval(curr.ShowedAt, curr.HiddenAt, next.ShowedAt, next.HiddenAt) < gap)
                {
                    curr.ShowedAt = curr.ShowedAt <= next.ShowedAt ? curr.ShowedAt : next.ShowedAt;
                    curr.HiddenAt = curr.HiddenAt >= next.HiddenAt ? curr.HiddenAt : next.HiddenAt;
                    curr.Slides.AddRange(next.Slides);
                    shows.RemoveAt(i + 1);
                    context.SongShows.Remove(next);
                    merged = true;
                    break;
                }
            }
            if (!merged)
            {
                break;
            }
        }
    }

    private static TimeSpan DistanceToInterval(DateTimeOffset aStart, DateTimeOffset aEnd, DateTimeOffset bStart,
        DateTimeOffset bEnd)
    {
        // Если пересекаются — 0
        if (aEnd >= bStart && aStart <= bEnd) return TimeSpan.Zero;

        // Иначе расстояние между ближайшими концами
        return aEnd < bStart ? (bStart - aEnd) : (aStart - bEnd);
    }
}