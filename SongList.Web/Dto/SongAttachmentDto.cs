using System.ComponentModel;
using SongList.Web.Entities;

namespace SongList.Web.Dto;

[DisplayName("Attachment")]
public class SongAttachmentDto
{
    public required int Id { get; init; }
    public required int SongId { get; init; }
    public required string Name { get; init; }
    public required AttachmentType Type { get; init; }
}