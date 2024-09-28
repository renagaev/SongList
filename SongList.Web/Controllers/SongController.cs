using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SongList.Web.Services;

namespace SongList.Web.Controllers;

[Route("[controller]")]
public class SongController(AppContext dbContext, OpenedSongsManager openedSongsManager) : ControllerBase
{
    [HttpGet(Name = "getAllSongs")]
    public Task<Song[]> GetAllSongs() => dbContext.Set<Song>().ToArrayAsync();

    [HttpGet("opened", Name = "getOpenedSongs")]
    public SongOpeningStats[] GetOpenedSongs() => openedSongsManager
        .GetOpenSongs()
        .Select(x =>
            new SongOpeningStats
            {
                Id = x.Key,
                Count = x.Value
            })
        .ToArray();
}