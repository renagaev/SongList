using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
        var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<SongHistory[]>>(jsonOptions, cancellationToken);
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
        var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<TitleResponse>>(jsonOptions, cancellationToken);
        return wrapper!.Data.Title;
    }
}