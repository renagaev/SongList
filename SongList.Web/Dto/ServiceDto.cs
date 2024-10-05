namespace SongList.Web.Dto;

public record ServiceDto
{
    public DateOnly Date { get; init; }
    public ServiceType Type { get; init; }
    public int[] Songs { get; init; }
}

public enum ServiceType
{
    Morning,
    Evening,
    Other
}