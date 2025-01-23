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
            NoteId = x.NoteId,
            Note = x.Note != null ? x.Note.SimpleName : null,
            Number = x.Number,
            Tags = x.Tags,
            Text = x.Text
        })
        .ToArrayAsync(cancellationToken);

    [HttpGet("notes", Name = "getNotes")]
    public Task<NoteDto[]> GetNotes(CancellationToken cancellationToken) => dbContext.Notes
        .Select(x => new NoteDto
        {
            Id = x.Id,
            Name = x.Name,
            SimpleName = x.SimpleName
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