namespace SongList.Holyrics.Interfaces;

public record HolyricsPartition(string Id, string Md5);

public interface IHolyricsSyncClient
{
    Task<List<HolyricsPartition>> GetPartitions(CancellationToken cancellationToken);

    Task<ICollection<HolyricsSyncSong>> GetSongs(string partitionId, CancellationToken cancellationToken);
}