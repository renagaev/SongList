using System.Text.RegularExpressions;

namespace SongList.Web.UseCases.SyncHolyricsSongs;

public class FormattingParser
{
    private static Regex Regex = new Regex(@"##\((?:Песнь [В|в]озрождения №(?<number>\d+))?\s*\-*\s*(?<note>\w+\#?♭?)?\)"); 
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