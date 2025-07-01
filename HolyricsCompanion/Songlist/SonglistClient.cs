using System.Net.Http.Json;

namespace HolyricsCompanion.Songlist;

public class SonglistClient(HttpClient httpClient)
{
    public async Task ReportSongSlide(SongSlideItem item, CancellationToken cancellationToken)
    {
        var res = await httpClient.PostAsJsonAsync("/history/slides", item, cancellationToken);
        res.EnsureSuccessStatusCode();
    }

    public async Task ReportBibleVerse(BibleVerseItem item, CancellationToken cancellationToken)
    {
        var res = await httpClient.PostAsJsonAsync("/bible-history", item, cancellationToken);
        res.EnsureSuccessStatusCode();
    }

    public async Task ReportHistoryItem(SongHistoryItem model, CancellationToken cancellationToken)
    {
        var res = await httpClient.PostAsJsonAsync("/history", model, cancellationToken);
        res.EnsureSuccessStatusCode();
    }
}