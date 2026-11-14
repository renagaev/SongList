using Microsoft.Extensions.Options;
using SongList.Holyrics.JavaHelper;

namespace SongList.Holyrics.Interfaces;

internal class HolyricsSyncClient(
    HolyricsDriveClient driveClient,
    MusicDecoder decoder,
    IOptions<HolyricsSyncOptions> options) : IHolyricsSyncClient
{
    public async Task<List<HolyricsPartitionDto>> GetPartitions(CancellationToken cancellationToken)
    {
        var files = await driveClient.ListFilesAsync(options.Value.DriveFolder, cancellationToken);
        return files
            .Where(x => HolyricsSyncUtils.IsMusicDbFileName(x.Name))
            .Select(x => new HolyricsPartitionDto(x.Id, GetMd5(x.AppProperties) ?? "", x.ModifiedAt))
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
            foreach (var line in value.Split('\n'))
            {
                var parts = line.Trim().Split('=', 2);
                if (parts.Length == 2 && parts[0] == "custom_md5")
                {
                    return parts[1];
                }
            }
        }

        return null;
    }
}
