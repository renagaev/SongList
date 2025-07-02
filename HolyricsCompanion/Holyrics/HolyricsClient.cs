using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace HolyricsCompanion.Holyrics;

public class HolyricsClient
{
    private readonly HolyricsSettings _settings;
    private JsonSerializerOptions jsonOptions;

    private readonly HttpClient _httpClient;

    public HolyricsClient(HttpClient httpClient, IOptionsSnapshot<HolyricsSettings> optionsSnapshot)
    {
        _httpClient = httpClient;
        _settings = optionsSnapshot.Value;
        jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        jsonOptions.Converters.Add(new DatetimeOffsetConverter());
    }

    public async Task<SongHistory[]> GetHistories(CancellationToken cancellationToken)
    {
        var url = $"{_settings.BaseUrl}/api/GetHistories?token={_settings.Token}";
        var content = JsonContent.Create(new object());
        await content.LoadIntoBufferAsync();
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        var wrapper =
            await response.Content.ReadFromJsonAsync<ResponseWrapper<SongHistory[]>>(jsonOptions, cancellationToken);
        return wrapper!.Data;
    }

    public async Task<string> GetTitle(string songId, CancellationToken cancellationToken)
    {
        var url = $"{_settings.BaseUrl}/api/GetSong?token={_settings.Token}";
        var content = JsonContent.Create(new TitleRequest { Id = songId });
        await content.LoadIntoBufferAsync();
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        var wrapper =
            await response.Content.ReadFromJsonAsync<ResponseWrapper<TitleResponse>>(jsonOptions, cancellationToken);
        return wrapper!.Data.Title;
    }

    public async Task<CurrentSlideDto?> GetCurrentSlide(CancellationToken cancellationToken)
    {
        var url = $"{_settings.BaseUrl}/api/GetCurrentPresentation?token={_settings.Token}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var wrapper =
            await response.Content.ReadFromJsonAsync<ResponseWrapper<CurrentSlideResponse>>(cancellationToken);
        if (wrapper!.Data == null)
        {
            return null;
        }

        var data = wrapper.Data!;

        if (data.Type is not ("song" or "verse"))
        {
            return null;
        }

        if (data.Type == "song")
        {
            var slide = GetSongSlide(data);
            return new CurrentSlideDto(SlideType.Song, null, slide);
        }

        if (data.Type == "verse")
        {
            var verse = GetVerse(data);
            return new CurrentSlideDto(SlideType.Bible, verse, null);
        }

        return null;
    }

    private static BibleVerse? GetVerse(CurrentSlideResponse response)
    {
        var match = Regex.Match(response.Id, @"(\d\d)(\d\d\d)(\d\d\d)");
        if (!match.Success)
        {
            return null;
        }

        var book = int.Parse(match.Groups[1].Value);
        var chapter = int.Parse(match.Groups[2].Value);
        var verse = int.Parse(match.Groups[3].Value);

        return new BibleVerse(book, chapter, verse);
    }

    private static SongSlide? GetSongSlide(CurrentSlideResponse response)
    {
        if (string.IsNullOrEmpty(response.SongId))
        {
            return null;
        }

        return new SongSlide(response.SongId, response.SongName, response.SlideNumber!.Value, response.TotalSlides!.Value);
    }
}