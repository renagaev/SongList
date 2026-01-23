using Microsoft.EntityFrameworkCore;
using SongList.Domain;
using SongList.Holyrics.Interfaces;
using SongList.Web.Services;

namespace SongList.Web.UseCases.SyncHolyricsSongs;

public class SyncHolyricsSongsHandler(AppContext context, IHolyricsSyncClient syncClient, SongUpdateNotifier tgNotifier)
{
    private FormattingParser _parser = new FormattingParser();

    public async Task Handle(CancellationToken cancellationToken)
    {
        var modified = await GetModifiedPartitions(cancellationToken);
        if (modified.Count == 0)
        {
            return;
        }

        var notes = context.Notes.Where(x => x.DetailedName.Contains("первая")).ToList();
        var imported = new List<Song>();

        foreach (var (dto, existing) in modified)
        {
            if (existing == null)
            {
                var newPartition = new HolyricsPartition
                {
                    Id = dto.Id,
                    Md5 = dto.Md5
                };
                context.HolyricsPartitions.Add(newPartition);
            }
            else
            {
                existing.Md5 = dto.Md5;
            }


            var songs = await syncClient.GetSongs(dto.Id, cancellationToken);
            var ids = songs.Select(x => x.Id.ToString()).ToList();
            var existingSongs = await context.HolyricsSongs
                .Where(x => ids.Contains(x.HolyricsId))
                .ToDictionaryAsync(x => x.HolyricsId, cancellationToken);


            foreach (var holyricsSyncSong in songs)
            {
                var existingSong = existingSongs.GetValueOrDefault(holyricsSyncSong.Id.ToString(), null);
                if (existingSong == null)
                {
                    existingSong = new HolyricsSong { HolyricsId = holyricsSyncSong.Id.ToString() };
                    context.HolyricsSongs.Add(existingSong);
                }

                existingSong.Title = ClearTitle(SongService.ReplaceLatinWithCyrillic(holyricsSyncSong.Title));
                existingSong.Lyrics = SongService.ReplaceLatinWithCyrillic(holyricsSyncSong.Lyrics);
                existingSong.Formatting = SongService.ReplaceLatinWithCyrillic(holyricsSyncSong.Formatting);
                if (existingSong.SongId == null)
                {
                    var parsed = _parser.Parse(existingSong.Formatting);
                    var note = notes.FirstOrDefault(x =>
                        x.SimpleName.Equals(parsed.Note?.ToLower(), StringComparison.CurrentCultureIgnoreCase));

                    var newSong = new Song
                    {
                        Title = existingSong.Title,
                        Text = existingSong.Lyrics,
                        Number = parsed.Number,
                        Tags = ["Новые"],
                        NoteId = note?.Id,
                        OriginalTitle = existingSong.Title
                    };
                    existingSong.Song = newSong;
                    context.Songs.Add(newSong);
                    imported.Add(newSong);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        if (imported.Count > 0)
        {
            await tgNotifier.NotifyNewSongsImported(imported.Select(x => x.Title).ToList(), cancellationToken);
        }
    }

    private string ClearTitle(string title)
    {
        var cleared = title.Replace("»", "").Replace("«", "").Trim();
        if (title.Length != 0)
        {
            cleared = cleared[0].ToString().ToUpper() + cleared[1..];
        }

        return cleared;
    }


    private async Task<List<(HolyricsPartitionDto modified, HolyricsPartition? existing)>> GetModifiedPartitions(
        CancellationToken cancellationToken)
    {
        var partitions = await syncClient.GetPartitions(cancellationToken);
        var currentState = await context.HolyricsPartitions.ToDictionaryAsync(x => x.Id, cancellationToken);

        var res = new List<(HolyricsPartitionDto, HolyricsPartition?)>();
        foreach (var dto in partitions)
        {
            if (currentState.TryGetValue(dto.Id, out var existing))
            {
                if (existing.Md5 != dto.Md5 && dto.UpdatedAt < DateTimeOffset.Now.AddMinutes(-5))
                {
                    res.Add((dto, existing));
                }
            }
            else
            {
                res.Add((dto, null));
            }
        }

        return res;
    }
}