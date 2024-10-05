using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SongList.Web.Dto;
using SongList.Web.Services;

namespace SongList.Web.Controllers;

[Route("history")]
public class HistoryController(HistoryService service, AppContext dbContext) : ControllerBase
{
    
    [Authorize(AuthenticationSchemes = "Token")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost]
    public async Task AddHistoryItem([FromBody] AddHistoryItemRequest request, CancellationToken cancellationToken)
    {
        await service.AddHistoryItem(request.HolyricsId, request.CreatedAt, request.Title, cancellationToken);
    }
    
    [HttpGet("{songId:int}", Name = "getSongHistory")]
    public Task<DateTimeOffset[]> GetSongHistory(int songId, CancellationToken cancellationToken) => dbContext.History
        .Where(x => x.SongId == songId)
        .Select(x => x.CreatedAt)
        .ToArrayAsync(cancellationToken);

    [HttpGet("services", Name = "getServices")]
    public Task<ServiceDto[]> GetServicesHistory(CancellationToken cancellationToken) =>
        service.GetServices(cancellationToken);
}