namespace SongList.Holyrics;

internal class HolyricsSyncWriter(HolyricsDriveClient drive, JavaSyncHelperApplier applier)
{
    private readonly HolyricsDriveClient _drive = drive ?? throw new ArgumentNullException(nameof(drive));
    private readonly JavaSyncHelperApplier _applier = applier ?? throw new ArgumentNullException(nameof(applier));

    public async Task ApplySongUpdatesAsync(
        IEnumerable<HolyricsSongUpdate> updates,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizeUpdates(updates).ToList();
        if (normalized.Count == 0)
        {
            return;
        }

        var files = await _drive
            .ListFilesAsync(HolyricsDriveClient.DefaultFolder, cancellationToken);

        var map = files.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
        var byPartition = normalized
            .GroupBy(u => HolyricsSyncUtils.GetPartitionIndex(u.SyncId!))
            .ToList();

        foreach (var group in byPartition)
        {
            var name = HolyricsSyncUtils.GetPartitionFileName(group.Key);
            if (!map.TryGetValue(name, out var file))
            {
                throw new InvalidOperationException($"Partition file not found: {name}");
            }

            var bytes = await _drive.DownloadFileAsync(file.Id, cancellationToken);
            var result = await _applier.ApplyUpdatesAsync(bytes, group.ToList(), cancellationToken);

            await _drive.UpdateFileAsync(file, result.Bytes, result.CustomMd5, cancellationToken);
        }
    }

    private static IEnumerable<HolyricsSongUpdate> NormalizeUpdates(IEnumerable<HolyricsSongUpdate> updates)
    {
        if (updates == null)
        {
            yield break;
        }

        foreach (var update in updates)
        {
            if (update == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(update.SyncId))
            {
                if (update.Id <= 0)
                {
                    update.Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
                update.SyncId = update.Id.ToString("x");
            }

            yield return update;
        }
    }
}
