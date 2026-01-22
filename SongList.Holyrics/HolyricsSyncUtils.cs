namespace SongList.Holyrics;

internal static class HolyricsSyncUtils
{
    private const string SyncBytesPrefix = "K8ABZWM4ZjYnxb1f5OXYZk9x0RbiRjlKwB33QrY9_";

    public static bool IsSyncBytesFileName(string name)
    {
        return !string.IsNullOrEmpty(name) &&
               name.StartsWith(SyncBytesPrefix, StringComparison.Ordinal);
    }

    public static string? GetSyncBytesName(string name)
    {
        return IsSyncBytesFileName(name) ? name[SyncBytesPrefix.Length..] : null;
    }

    public static bool IsMusicDbFileName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }
        var shortName = GetSyncBytesName(name) ?? name;
        return shortName.StartsWith("music_", StringComparison.OrdinalIgnoreCase) &&
               shortName.EndsWith(".db", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsDeletedItemsFileName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }
        var shortName = GetSyncBytesName(name) ?? name;
        return shortName.Equals("deleted_items.dat", StringComparison.OrdinalIgnoreCase);
    }

    public static int GetPartitionIndex(string syncId)
    {
        if (string.IsNullOrWhiteSpace(syncId))
        {
            return 0;
        }
        var c = syncId[^1];
        return c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'a' and <= 'f' => 10 + (c - 'a'),
            >= 'A' and <= 'F' => 10 + (c - 'A'),
            _ => c % 16
        };
    }

    private static string GetPartitionName(int index)
    {
        if (index < 0) index = 0;
        if (index > 15) index = 15;
        return $"music_{index}.db";
    }

    public static string GetPartitionFileName(int index)
    {
        return SyncBytesPrefix + GetPartitionName(index);
    }
}
