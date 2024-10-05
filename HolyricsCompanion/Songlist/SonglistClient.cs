using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace HolyricsCompanion.Songlist;

public class SonglistClient(HttpClient httpClient, IOptionsSnapshot<SonglistSettings> optionsSnapshot)
{
    private readonly SonglistSettings _settings = optionsSnapshot.Value;

    public async Task ReportItem(string holyricsId, DateTimeOffset createdAt, string title, CancellationToken cancellationToken)
    {
        var model = new HistoryItem
        {
            HolyricsId = holyricsId,
            CreatedAt = createdAt,
            Title = title
        };

        var req = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/history")
        {
            Content = JsonContent.Create(model)
        };
        req.Headers.Authorization = new AuthenticationHeaderValue(_settings.Token);
        var res = await httpClient.SendAsync(req, cancellationToken);
        res.EnsureSuccessStatusCode();
    }
}