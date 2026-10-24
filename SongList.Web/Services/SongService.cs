using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SongList.Domain;
using SongList.Web.Dto;
using SongList.Web.Extensions;

namespace SongList.Web.Services;

public class SongService(AppContext dbContext, SongUpdateNotifier notifier, SongHistoryService historyService)
{
    private static Expression<Func<Song, SongDto>> projection = x => new SongDto
    {
        Id = x.Id,
        Title = x.Title,
        NoteId = x.NoteId,
        Note = x.Note != null ? x.Note.SimpleName : null,
        Number = x.Number,
        Tags = x.Tags,
        Text = x.Text,
        Attachments = x.Attachments
            .Select(a => new SongAttachmentDto
            {
                Id = a.Id,
                SongId = a.SongId,
                Name = a.Name,
                Type = a.Type
            })
            .ToArray()
    };

    public async Task<SongDto[]> GetAllSongs(bool includeHistory, CancellationToken cancellationToken)
    {
        var songs = await dbContext.Songs
            .Where(x => !x.IsDeleted)
            .Select(projection)
            .ToArrayAsync(cancellationToken);

        if (includeHistory)
        {
            var songIds = songs.Select(x => x.Id).ToArray();
            var lastHistory = await historyService.GetLastHistoryMap(songIds, cancellationToken);
            foreach (var song in songs)
            {
                if (lastHistory.TryGetValue(song.Id, out var last))
                {
                    song.LastMorning = last.LastMorning;
                    song.LastEvening = last.LastEvening;
                }
            }
        }

        return songs;
    }

    public async Task<SongDto> UpdateSong(int id, SongDto songDto, string userName, CancellationToken cancellationToken)
    {
        songDto.Text = songDto.Text.ReplaceLatin();
        songDto.Title = songDto.Title.ReplaceLatin();
        
        var song = await dbContext.Songs.FirstAsync(x => x.Id == id, cancellationToken);

        var old = new SongDto
        {
            Text = song.Text,
            Title = song.Title,
            Tags = song.Tags,
            NoteId = song.NoteId,
            Number = song.Number
        };
        song.NoteId = songDto.NoteId;
        song.Tags = songDto.Tags;
        song.Number = songDto.Number;
        song.Text = songDto.Text;
        song.Title = songDto.Title;
        await dbContext.SaveChangesAsync(cancellationToken);
        await notifier.NofifyUpdate(old, songDto, userName, cancellationToken);
        return await dbContext.Songs.Select(projection).FirstAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<SongDto> AddSong(SongDto songDto, CancellationToken cancellationToken)
    {
        var song = new Song
        {
            Text = songDto.Text.ReplaceLatin(),
            Title = songDto.Title.ReplaceLatin(),
            OriginalTitle = songDto.Title,
            Tags = songDto.Tags,
            Number = songDto.Number,
            NoteId = songDto.NoteId
        };
        dbContext.Songs.Add(song);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await dbContext.Songs.Select(projection).FirstAsync(x => x.Id == song.Id, cancellationToken);
    }

    public async Task DeleteSong(int id, string userName, CancellationToken cancellationToken)
    {
        var song = await dbContext.Songs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (song is null || song.IsDeleted)
        {
            return;
        }

        song.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        await notifier.NotifySongDeleted(song.Title, userName, cancellationToken);
    }
    

}
