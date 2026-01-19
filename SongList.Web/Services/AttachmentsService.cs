using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using SongList.Domain;
using SongList.Web.Dto;

namespace SongList.Web.Services;

public class AttachmentsService(AppContext dbContext, IMinioClient minioClient, IOptionsSnapshot<S3Settings> s3Settings)
{
    public async Task<SongAttachmentDto[]> GetAttachments(int songId, CancellationToken cancellationToken)
    {
        return await dbContext.Attachments
            .Where(x => x.SongId == songId)
            .Select(x => new SongAttachmentDto
            {
                Id = x.Id,
                Name = x.Name,
                SongId = x.SongId,
                Type = x.Type
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<(string name, Stream content)> GetContent(int id, CancellationToken cancellationToken)
    {
        var attachment = await dbContext.Attachments
            .FirstAsync(x => x.Id == id, cancellationToken);

        var ms = new MemoryStream();

        var args = new GetObjectArgs()
            .WithObject(attachment.S3Path)
            .WithBucket(s3Settings.Value.BucketName)
            .WithCallbackStream((stream, ct) => stream.CopyToAsync(ms, ct));

        await minioClient.GetObjectAsync(args, cancellationToken);
        ms.Position = 0;
        return (attachment.Name, ms);
    }

    public async Task CreateAttachment(int songId, Stream stream, string title, string originalName,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(originalName);
        var type = extension switch
        {
            ".mp3" or ".wav" or ".ogg" or ".flac" => AttachmentType.Audio,
            _ => AttachmentType.Document
        };
        var attachment = new SongAttachment
        {
            SongId = songId,
            Name = title + extension,
            Type = type,
            S3Path = $"{songId}/{Guid.NewGuid()}{Path.GetExtension(originalName)}"
        };
        dbContext.Attachments.Add(attachment);

        var args = new PutObjectArgs()
            .WithObject(attachment.S3Path)
            .WithBucket(s3Settings.Value.BucketName)
            .WithObjectSize(stream.Length)
            .WithStreamData(stream);
        await minioClient.PutObjectAsync(args, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAttachment(int id, CancellationToken cancellationToken)
    {
        var attachment = await dbContext.Attachments
            .FirstAsync(x => x.Id == id, cancellationToken);
        attachment.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}