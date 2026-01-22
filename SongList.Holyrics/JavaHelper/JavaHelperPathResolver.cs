namespace SongList.Holyrics.JavaHelper;

internal static class JavaHelperPathResolver
{
    internal const string DefaultClassPath = "java-decoder";

    public static string ResolveClassPath()
    {
        var normalized = DefaultClassPath;

        if (normalized.Contains(Path.PathSeparator))
        {
            return normalized;
        }

        var trimmed = normalized.Trim();
        if (Path.IsPathRooted(trimmed))
        {
            return trimmed;
        }

        var candidate = Path.GetFullPath(trimmed);
        if (Directory.Exists(candidate))
        {
            return candidate;
        }

        var baseCandidate = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, trimmed));
        if (Directory.Exists(baseCandidate))
        {
            return baseCandidate;
        }

        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var direct = Path.Combine(current.FullName, trimmed);
            if (Directory.Exists(direct))
            {
                return direct;
            }

            var withProject = Path.Combine(current.FullName, "SongList.Holyrics", trimmed);
            if (Directory.Exists(withProject))
            {
                return withProject;
            }

            current = current.Parent;
        }

        return candidate;
    }
}