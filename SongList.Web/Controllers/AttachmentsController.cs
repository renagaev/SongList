using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SongList.Web.Dto;
using SongList.Web.Services;

namespace SongList.Web.Controllers;

[Route("attachments")]
public class AttachmentsController(AttachmentsService service) : ControllerBase
{
    [HttpGet("song/{songId:int}", Name = "getAttachments")]
    public async Task<SongAttachmentDto[]> GetAttachments(int songId, CancellationToken cancellationToken)
    {
        return await service.GetAttachments(songId, cancellationToken);
    }

    [HttpGet("{id:int}/download", Name = "getAttachment")]
    public async Task<IActionResult> GetAttachment(int id, CancellationToken cancellationToken)
    {
        var attachment = await service.GetContent(id, cancellationToken);
        return new FileContentResult(attachment.content, "application/octet-stream")
        {
            FileDownloadName = attachment.name
        };
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("song/{songId:int}", Name = "uploadAttachment")]
    public async Task UploadAttachment(int songId, IFormFile file, [FromForm] string displayName, CancellationToken cancellationToken)
    {
        var stream = file.OpenReadStream();
        await service.CreateAttachment(songId, stream, displayName, file.Name, cancellationToken);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpDelete("{id:int}", Name = "deleteAttachment")]
    public async Task DeleteAttachment(int id, CancellationToken cancellationToken)
    {
        await service.DeleteAttachment(id, cancellationToken);
    }
}