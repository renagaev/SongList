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

    public async Task<(string name, Stream content)?> GetContent(int id, CancellationToken cancellationToken)
    {
        var attachment = await dbContext.Attachments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (attachment is null)
        {
            return null;
        }

        var ms = new MemoryStream();

        var args = new GetObjectArgs()
            .WithObject(attachment.S3Path)
            .WithBucket(s3Settings.Value.BucketName)
            .WithCallbackStream((stream, ct) => stream.CopyToAsync(ms, ct));

        await minioClient.GetObjectAsync(args, cancellationToken);
        ms.Position = 0;
        return (attachment.Name, ms);
    }

    public async Task<bool> CreateAttachment(int songId, Stream stream, string title, string originalName,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Songs.AnyAsync(x => x.Id == songId && !x.IsDeleted, cancellationToken))
        {
            return false;
        }

        var extension = Path.GetExtension(originalName);
        var type = extension.ToLowerInvariant() switch
        {
            ".mp3" or ".wav" or ".ogg" or ".flac" or ".opus" or ".aac" or ".m4a" => AttachmentType.Audio,
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
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Не оставляем сиротский объект в S3, если запись в БД не удалась.
            try
            {
                await minioClient.RemoveObjectAsync(
                    new RemoveObjectArgs().WithBucket(s3Settings.Value.BucketName).WithObject(attachment.S3Path),
                    CancellationToken.None);
            }
            catch
            {
                // Не маскируем исходную ошибку БД ошибкой очистки.
            }

            throw;
        }

        return true;
    }

    public async Task<bool> DeleteAttachment(int id, CancellationToken cancellationToken)
    {
        var attachment = await dbContext.Attachments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (attachment is null)
        {
            return false;
        }

        attachment.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}