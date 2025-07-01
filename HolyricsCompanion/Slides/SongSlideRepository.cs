using LiteDB;

namespace HolyricsCompanion.Slides;

public class SongSlideRepository(LiteDatabase db)
{
    private readonly ILiteCollection<SongSlideShowRecord> _collection = db.GetCollection<SongSlideShowRecord>();

    public void Upsert(SongSlideShowRecord record) => _collection.Upsert(record);

    public SongSlideShowRecord[] GetAll() => _collection.Query().ToArray();
    
    public void Remove(SongSlideShowRecord record) => _collection.Delete(record.Id);
}