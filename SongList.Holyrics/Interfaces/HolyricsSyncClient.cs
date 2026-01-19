using Microsoft.Extensions.Options;
using SongList.Holyrics.JavaHelper;

namespace SongList.Holyrics.Interfaces;

internal class HolyricsSyncClient(
    HolyricsDriveClient driveClient,
    MusicDecoder decoder,
    IOptions<HolyricsSyncOptions> options) : IHolyricsSyncClient
{
    public async Task<List<HolyricsPartition>> GetPartitions(CancellationToken cancellationToken)
    {
        var files = await driveClient.ListFilesAsync(options.Value.DriveFolder, cancellationToken);
        return files
            .Where(x => HolyricsSyncUtils.IsMusicDbFileName(x.Name))
            .Select(x => new HolyricsPartition(x.Id, GetMd5(x.AppProperties) ?? ""))
            .ToList();
    }

    public async Task<ICollection<HolyricsSyncSong>> GetSongs(string partitionId, CancellationToken cancellationToken)
    {
        var content = await driveClient.DownloadFileAsync(partitionId, cancellationToken);
        return await decoder.DecodeAsync(content, cancellationToken);
    }

    private string? GetMd5(IDictionary<string, string> appProperties)
    {
        if (appProperties.TryGetValue("s1", out var value))
        {
            return value.Split("\n").ToDictionary(x => x.Split("=")[0], x => x.Split("=")[1])
                .GetValueOrDefault("custom_md5", null);
        }

        return null;
    }
}
