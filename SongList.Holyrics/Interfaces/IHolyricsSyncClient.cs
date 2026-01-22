namespace SongList.Holyrics.Interfaces;

public record HolyricsPartitionDto(string Id, string Md5, DateTimeOffset? UpdatedAt);

public interface IHolyricsSyncClient
{
    Task<List<HolyricsPartitionDto>> GetPartitions(CancellationToken cancellationToken);

    Task<ICollection<HolyricsSyncSong>> GetSongs(string partitionId, CancellationToken cancellationToken);
}