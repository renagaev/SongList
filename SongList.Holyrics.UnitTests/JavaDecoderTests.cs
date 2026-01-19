using SongList.Holyrics.JavaHelper;

namespace SongList.Holyrics.UnitTests;

public class JavaDecoderTests
{
    [Fact]
    public async Task Should_Decode_Music()
    {
        // Arrange
        var bytes = await File.ReadAllBytesAsync("Files/music.db");
        var decoder = new MusicDecoder();
        
        // Act
        var songs = await decoder.DecodeAsync(bytes, CancellationToken.None);
        
        // Assert
        Assert.Equal(43, songs.Count);
    }
}