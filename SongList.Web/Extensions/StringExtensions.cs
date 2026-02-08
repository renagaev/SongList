using System.Text;

namespace SongList.Web.Extensions;

public static class StringExtensions
{
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
        ['b'] = 'Ь' // иногда используют так, тоже опционально
    };


    public static string ReplaceLatin(this string input)
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