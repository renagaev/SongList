using System.Text.RegularExpressions;
using SongList.Domain;
using SongList.Holyrics;
using SongList.Web.Extensions;

namespace SongList.Web.UseCases.SyncHolyricsSongs;

public record SyncSong(string Title, string Text, int? Number, string? Note);

public class HolyricsConverter
{
    public SyncSong Convert(HolyricsSyncSong holyricsSong)
    {
        var title = ClearTitle(holyricsSong.Title.ReplaceLatin());
        var lyrics = holyricsSong.Lyrics.ReplaceLatin();
        var (number, note) = Parse(holyricsSong.Formatting.ReplaceLatin());

        return new SyncSong(title, lyrics, number, note);
    }

    public SyncSong Convert(HolyricsSong holyricsSong)
    {
        var title = ClearTitle(holyricsSong.Title.ReplaceLatin());
        var lyrics = holyricsSong.Lyrics.ReplaceLatin();
        var (number, note) = Parse(holyricsSong.Formatting.ReplaceLatin());

        return new SyncSong(title, lyrics, number, note);
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

    private static Regex Regex =
        new Regex(@"##\((?:Песнь [В|в]озрождения №(?<number>\d+))?\s*\-*\s*(?<note>\w+\#?♭?)?\)");

    public (int? Number, string? Note) Parse(string formatting)
    {
        var firstSlide = formatting.Split("\n").FirstOrDefault();
        if (firstSlide == null)
        {
            return (null, null);
        }

        var match = Regex.Match(firstSlide);
        if (!match.Success)
        {
            return (null, null);
        }

        int? number = null;
        if (int.TryParse(match.Groups["number"].Value, out var parsed))
        {
            number = parsed;
        };
        var note = match.Groups["note"].Value;
        if (note.Length == 0)
        {
            note = null;
        }

        return (number, note);
    } 
}