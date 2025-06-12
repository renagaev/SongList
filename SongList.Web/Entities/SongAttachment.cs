namespace SongList.Web.Entities;

public enum AttachmentType
{
    Audio,
    Document
}

public class SongAttachment
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string S3Path { get; init; }
    public int SongId { get; init; }
    public Song Song { get; init; }
    public AttachmentType Type { get; init; }
    public bool IsDeleted { get; set; }
}