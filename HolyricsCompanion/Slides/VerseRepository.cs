using LiteDB;

namespace HolyricsCompanion.Slides;

public class VerseRepository(LiteDatabase db)
{
    private readonly ILiteCollection<VerseShowRecord> _collection = db.GetCollection<VerseShowRecord>();

    public void Upsert(VerseShowRecord verseShowRecord) => _collection.Upsert(verseShowRecord);

    public VerseShowRecord[] GetAll() => _collection.Query().ToArray();
    
    public void Remove(VerseShowRecord verseShowRecord) => _collection.Delete(verseShowRecord.Id);
}