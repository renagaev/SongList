using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
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
        string contentType;
        new FileExtensionContentTypeProvider().TryGetContentType(attachment.name, out contentType);
        contentType ??= "application/octet-stream";
        Response.Headers.ContentDisposition = "inline; filename=" + HttpUtility.UrlEncode(attachment.name);
        return new FileStreamResult(attachment.content, contentType)
        {
            FileDownloadName = attachment.name,
            EnableRangeProcessing = true
        };
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("song/{songId:int}", Name = "uploadAttachment")]
    public async Task UploadAttachment(int songId, IFormFile file, [FromForm] string displayName,
        CancellationToken cancellationToken)
    {
        var stream = file.OpenReadStream();
        await service.CreateAttachment(songId, stream, displayName, file.FileName, cancellationToken);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpDelete("{id:int}", Name = "deleteAttachment")]
    public async Task DeleteAttachment(int id, CancellationToken cancellationToken)
    {
        await service.DeleteAttachment(id, cancellationToken);
    }
}