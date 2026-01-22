using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SongList.Holyrics.JavaHelper;

public sealed class MusicDecoder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string _javaCommand = "java";
    private readonly string _classPath = JavaHelperPathResolver.ResolveClassPath();
    private const string _mainClass = "com.holyrics.sync.HolyricsSyncHelper";
    private const string _javaEncodingOption = "-Dfile.encoding=UTF-8";

    public async Task<ICollection<HolyricsSyncSong>> DecodeAsync(byte[] bytes, CancellationToken cancellationToken)
    {
        var args = new List<string>
        {
            _javaEncodingOption,
            "-cp",
            _classPath,
            _mainClass,
            "--stdin"
        };

        var psi = new ProcessStartInfo
        {
            FileName = _javaCommand,
            Arguments = string.Join(" ", args.Select(Quote)),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start Java helper.");
        }

        await WriteInputAsync(process.StandardInput.BaseStream, bytes, cancellationToken);
        process.StandardInput.Close();

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Java helper failed: {stderr}".Trim());
        }

        return JsonSerializer.Deserialize<ICollection<HolyricsSyncSong>>(stdout, JsonOptions)!;
    }

    public async Task<IReadOnlyList<HolyricsDeletedItem>> DecodeDeletedAsync(
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        var args = new List<string>
        {
            _javaEncodingOption,
            "-cp",
            _classPath,
            _mainClass,
            "--deleted",
            "--stdin"
        };

        var psi = new ProcessStartInfo
        {
            FileName = _javaCommand,
            Arguments = string.Join(" ", args.Select(Quote)),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start Java helper.");
        }

        await WriteDeletedInputAsync(process.StandardInput.BaseStream, bytes, cancellationToken);
        process.StandardInput.Close();

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Java helper failed: {stderr}".Trim());
        }

        var payload = JsonSerializer.Deserialize<HolyricsDeletedItemsPayload>(stdout, JsonOptions);
        return payload?.Deleted ?? new List<HolyricsDeletedItem>();
    }

    private static async Task WriteInputAsync(
        Stream stream,
        byte[]? data,
        CancellationToken cancellationToken)
    {
        data ??= [];
        await WriteInt32Async(stream, data.Length, cancellationToken);
        await stream.WriteAsync(data, cancellationToken);
    }

    private static async Task WriteDeletedInputAsync(
        Stream stream,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        await WriteInt32Async(stream, bytes.Length, cancellationToken);
        await stream.WriteAsync(bytes, cancellationToken);
    }

    private static async Task WriteInt32Async(Stream stream, int value, CancellationToken cancellationToken)
    {
        var buffer = new byte[4];
        buffer[0] = (byte)((value >> 24) & 0xFF);
        buffer[1] = (byte)((value >> 16) & 0xFF);
        buffer[2] = (byte)((value >> 8) & 0xFF);
        buffer[3] = (byte)(value & 0xFF);
        await stream.WriteAsync(buffer, cancellationToken);
    }

    private static string Quote(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "\"\"";
        }

        return value.Contains(' ') || value.Contains('"')
            ? "\"" + value.Replace("\"", "\\\"") + "\""
            : value;
    }
}
