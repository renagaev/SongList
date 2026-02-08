using SongList.Web.UseCases.SyncHolyricsSongs;

namespace SongList.Holyrics.UnitTests;

public class HolyricsConverterTests
{
    [Theory]
    [InlineData("", null, null)]
    [InlineData("##(Песнь возрождения №497 - До)", 497, "До")]
    [InlineData("##(Ля)", null, "Ля")]
    [InlineData("##(Песнь возрождения №946)", 946, null)]
    [InlineData("##(Песнь возрождения №22 - соль#)", 22, "соль#")]
    [InlineData("##(Песнь Возрождения №22 - соль#)", 22, "соль#")]
    [InlineData("##(Песнь Возрождения №22 соль#)", 22, "соль#")]
    [InlineData("##(Песнь Возрождения №22 соль♭)", 22, "соль♭")]
    public void ShouldParse(string firstSlide, int? expectedNumber, string? expectedNote)
    {
        var res = new HolyricsConverter().Parse(firstSlide);
        
        Assert.Equal(expectedNote, res.Note);
        Assert.Equal(expectedNumber, res.Number);
    }
}