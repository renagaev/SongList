using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SongList.Web.Dto;
using SongList.Web.Services;

namespace SongList.Web.Controllers;

[Route("bible-history")]
public class BibleHistoryController(VerseHistoryService service): ControllerBase
{
    [Authorize(AuthenticationSchemes = "Token")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost]
    public async Task AddHistoryItem([FromBody] AddVerseHistoryItemRequest request, CancellationToken cancellationToken)
    {
        await service.AddHistoryItem(request, cancellationToken);
    }
}