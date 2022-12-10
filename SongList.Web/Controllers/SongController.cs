using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SongList.Web.Controllers;

[Route("[controller]")]
public class SongController : ControllerBase
{
    private readonly AppContext _dbContext;

    public SongController(AppContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet(Name = "getAllSongs")]
    public Task<Song[]> GetAllSongs() => _dbContext.Set<Song>().ToArrayAsync();


    [HttpPost("import")]
    public async Task Import([FromBody] List<Song> songs)
    {
        _dbContext.AddRange(songs);
        await _dbContext.SaveChangesAsync();
    }
}