using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SongList.Web.Dto;
using SongList.Web.Services;

namespace SongList.Web.Controllers;

[ApiController]
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
        var result = await service.GetContent(id, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        var attachment = result.Value;
        new FileExtensionContentTypeProvider().TryGetContentType(attachment.name, out var contentType);
        contentType ??= "application/octet-stream";
        var disposition = new ContentDispositionHeaderValue("inline");
        disposition.SetHttpFileName(attachment.name);
        Response.Headers.ContentDisposition = disposition.ToString();
        return new FileStreamResult(attachment.content, contentType)
        {
            EnableRangeProcessing = true
        };
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("song/{songId:int}", Name = "uploadAttachment")]
    public async Task<IActionResult> UploadAttachment(int songId, IFormFile file, [FromForm] string displayName,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var created = await service.CreateAttachment(songId, stream, displayName, file.FileName, cancellationToken);
        return created ? Ok() : NotFound();
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpDelete("{id:int}", Name = "deleteAttachment")]
    public async Task<IActionResult> DeleteAttachment(int id, CancellationToken cancellationToken)
    {
        return await service.DeleteAttachment(id, cancellationToken) ? Ok() : NotFound();
    }
}