using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SongList.Web.Dto;
using SongList.Web.Entities;
using SongList.Web.Services;

namespace SongList.Web.Controllers;

[Route("[controller]")]
public class SongController(AppContext dbContext, OpenedSongsManager openedSongsManager, SongService songService)
    : ControllerBase
{
    [HttpGet(Name = "getAllSongs")]
    public async Task<SongDto[]> GetAllSongs(CancellationToken cancellationToken) =>
        await songService.GetAllSongs(cancellationToken);

    [HttpGet("notes", Name = "getNotes")]
    public Task<NoteDto[]> GetNotes(CancellationToken cancellationToken) => dbContext.Notes
        .Select(x => new NoteDto
        {
            Id = x.Id,
            Name = x.Name,
            SimpleName = x.SimpleName
        })
        .ToArrayAsync(cancellationToken);

    [HttpPost("{id}/edit", Name = "updateSong")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<SongDto> UpdateSong(int id, [FromBody] SongDto songDto, CancellationToken cancellationToken) =>
        await songService.UpdateSong(id, songDto, cancellationToken);

    [HttpPost("add", Name = "addSong")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<SongDto> AddSong([FromBody] SongDto songDto, CancellationToken cancellationToken) =>
        await songService.AddSong(songDto, cancellationToken);

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