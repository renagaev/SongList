using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SongList.Web.Dto;
using SongList.Web.Entities;
using SongList.Web.Services;

namespace SongList.Web.Controllers;

[Route("[controller]")]
public class SongController(AppContext dbContext, OpenedSongsManager openedSongsManager) : ControllerBase
{
    [HttpGet(Name = "getAllSongs")]
    public Task<SongDto[]> GetAllSongs(CancellationToken cancellationToken) => dbContext.Songs
        .Select(x => new SongDto
        {
            Id = x.Id,
            Title = x.Title,
            Note = x.Note,
            Number = x.Number,
            Tags = x.Tags,
            Text = x.Text
        })
        .ToArrayAsync(cancellationToken);

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