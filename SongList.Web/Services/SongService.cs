using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SongList.Web.Dto;
using SongList.Web.Entities;

namespace SongList.Web.Services;

public class SongService(AppContext dbContext)
{
    private static Expression<Func<Song, SongDto>> projection = x => new SongDto
    {
        Id = x.Id,
        Title = x.Title,
        NoteId = x.NoteId,
        Note = x.Note != null ? x.Note.SimpleName : null,
        Number = x.Number,
        Tags = x.Tags,
        Text = x.Text
    };

    public async Task<SongDto[]> GetAllSongs(CancellationToken cancellationToken)
    {
        return await dbContext.Songs
            .Select(projection)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<SongDto> UpdateSong(int id, SongDto songDto, CancellationToken cancellationToken)
    {
        var song = await dbContext.Songs.FirstAsync(x => x.Id == id, cancellationToken);
        song.Text = songDto.Text;
        song.Title = songDto.Title;
        song.NoteId = songDto.NoteId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await dbContext.Songs.Select(projection).FirstAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<SongDto> AddSong(SongDto songDto, CancellationToken cancellationToken)
    {
        var song = new Song
        {
            Text = songDto.Text,
            Title = songDto.Title,
            Tags = [],
            Number = songDto.Number,
            NoteId = songDto.NoteId
        };
        dbContext.Songs.Add(song);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await dbContext.Songs.Select(projection).FirstAsync(x => x.Id == song.Id, cancellationToken);
    }
}