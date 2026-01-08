using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SongList.Web.Dto;
using SongList.Web.Services;

namespace SongList.Web.Controllers;

[Route("history")]
public class HistoryController(SongHistoryService service, AppContext dbContext) : ControllerBase
{
    
    [Authorize(AuthenticationSchemes = "Token")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost]
    public async Task AddHistoryItem([FromBody] AddHistoryItemRequest request, CancellationToken cancellationToken) => 
        await service.AddHistoryItem(request.HolyricsId, request.CreatedAt, request.Title, cancellationToken);

    [Authorize(AuthenticationSchemes = "Token")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("slides")]
    public async Task AddSlideHistoryItem([FromBody] AddSlideHistoryItemRequest request, CancellationToken cancellationToken) => 
        await service.AddSlideHistoryItem(request, cancellationToken);

    [HttpGet("{songId:int}", Name = "getSongHistory")]
    public Task<DateTimeOffset[]> GetSongHistory(int songId, CancellationToken cancellationToken) =>
        service.GetSongHistory(songId, cancellationToken);

    [HttpGet("services", Name = "getServices")]
    public Task<ServiceDto[]> GetServicesHistory(CancellationToken cancellationToken) =>
        service.GetServices(cancellationToken);

    [HttpGet("last-songs", Name = "getSongsLastHistory")]
    public Task<SongLastHistoryDto[]> GetSongsLastHistory(CancellationToken cancellationToken) =>
        service.GetLastSongHistory(cancellationToken);
}
