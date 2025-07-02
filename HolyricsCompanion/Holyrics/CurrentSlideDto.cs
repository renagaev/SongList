namespace HolyricsCompanion.Holyrics;

public enum SlideType
{
    Song,
    Bible
}

public record CurrentSlideDto(SlideType Type, BibleVerse? Verse, SongSlide? SongSlide);

public record BibleVerse(int Book, int Chapter, int Verse);

public record SongSlide(string HolyricsId, string SongName, int SlideNumber, int TotalSlides);