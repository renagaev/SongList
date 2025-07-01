using LiteDB;

namespace HolyricsCompanion.History;

public class HistoryRepository: IDisposable
{
    private readonly LiteDatabase _db;
    private readonly ILiteCollection<HistoryItem> _collection;

    public HistoryRepository(LiteDatabase db)
    {
        _db = db;
        _collection = _db.GetCollection<HistoryItem>();
    }
    
    public HistoryItem[] GetNonSentItems()
    {
        return _collection.Query().Where(x => !x.Sent).ToArray();
    }

    public DateTimeOffset GetSongLastTime(string holyricsId)
    {
        var items = _collection
            .Query()
            .Where(x => x.HolyricsId == holyricsId)
            .ToList();
        return items.Count == 0 ? DateTimeOffset.MinValue : items.Select(x=> x.CreatedAt).Max();
    }

    public void Upsert(HistoryItem historyItem)
    {
        _collection.Upsert(historyItem);
    }
    

    public void Dispose()
    {
        _db.Dispose();
    }
}