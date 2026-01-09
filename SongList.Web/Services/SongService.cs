using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SongList.Web.Dto;
using SongList.Web.Entities;

namespace SongList.Web.Services;

public class SongService(AppContext dbContext, SongUpdateNotifier notifier)
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
            .Where(x=> !x.IsDeleted)
            .Select(projection)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<SongDto> UpdateSong(int id, SongDto songDto, string userName, CancellationToken cancellationToken)
    {
        songDto.Text = ReplaceLatinWithCyrillic(songDto.Text);
        songDto.Title = ReplaceLatinWithCyrillic(songDto.Title);
        
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
        await dbContext.SaveChangesAsync(cancellationToken);
        await notifier.NofifyUpdate(old, songDto, userName, cancellationToken);
        return await dbContext.Songs.Select(projection).FirstAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<SongDto> AddSong(SongDto songDto, CancellationToken cancellationToken)
    {
        var song = new Song
        {
            Text = ReplaceLatinWithCyrillic(songDto.Text),
            Title = ReplaceLatinWithCyrillic(songDto.Title),
            OriginalTitle = songDto.Title,
            Tags = songDto.Tags,
            Number = songDto.Number,
            NoteId = songDto.NoteId
        };
        dbContext.Songs.Add(song);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await dbContext.Songs.Select(projection).FirstAsync(x => x.Id == song.Id, cancellationToken);
    }
    
    private static readonly Dictionary<char, char> Map = new()
    {
        // Заглавные
        ['A'] = 'А',
        ['B'] = 'В',
        ['C'] = 'С',
        ['E'] = 'Е',
        ['H'] = 'Н',
        ['K'] = 'К',
        ['M'] = 'М',
        ['O'] = 'О',
        ['P'] = 'Р',
        ['T'] = 'Т',
        ['X'] = 'Х',
        ['Y'] = 'У',

        // Строчные
        ['a'] = 'а',
        ['c'] = 'с',
        ['e'] = 'е',
        ['o'] = 'о',
        ['p'] = 'р',
        ['x'] = 'х',
        ['y'] = 'у',
        ['k'] = 'к',
        ['m'] = 'м',
        ['h'] = 'һ', // похожее, но реже нужно — можно убрать
        ['b'] = 'Ь'  // иногда используют так, тоже опционально
    };

    public static string ReplaceLatinWithCyrillic(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder(input.Length);

        foreach (var ch in input)
        {
            sb.Append(Map.GetValueOrDefault(ch, ch));
        }

        return sb.ToString();
    }
}