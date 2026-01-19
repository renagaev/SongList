using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SongList.Holyrics.JavaHelper;

namespace SongList.Holyrics;

internal sealed class JavaSyncHelperApplier()
{
    private readonly string _javaCommand = "java";
    private readonly string _classPath = JavaHelperPathResolver.ResolveClassPath();
    private readonly string _mainClass = "com.holyrics.sync.HolyricsSyncHelper";

    public async Task<HolyricsApplyResult> ApplyUpdatesAsync(
        byte[] bytes,
        IReadOnlyList<HolyricsSongUpdate> updates,
        CancellationToken cancellationToken)
    {
        if (updates == null || updates.Count == 0)
        {
            throw new ArgumentException("Updates are required.", nameof(updates));
        }

        var args = new List<string>
        {
            "-cp",
            _classPath,
            _mainClass,
            "--apply",
            "--stdin"
        };

        var psi = new ProcessStartInfo
        {
            FileName = _javaCommand,
            Arguments = string.Join(" ", args.Select(Quote)),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start Java helper.");
        }

        await WriteApplyInputAsync(process.StandardInput.BaseStream, bytes, updates, cancellationToken);
        process.StandardInput.Close();

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Java helper failed: {stderr}".Trim());
        }

        var parsed = JsonSerializer.Deserialize<HolyricsApplyResultJson>(stdout);
        if (parsed == null || string.IsNullOrWhiteSpace(parsed.BytesBase64))
        {
            throw new InvalidOperationException("Invalid apply result.");
        }

        var resBytes = Convert.FromBase64String(parsed.BytesBase64);
        return new HolyricsApplyResult(parsed.Name, parsed.CustomMd5 ?? "", resBytes);
    }

    private static async Task WriteApplyInputAsync(
        Stream stream,
        byte[] bytes,
        IReadOnlyList<HolyricsSongUpdate> updates,
        CancellationToken cancellationToken)
    {
        await WriteInt32Async(stream, 1, cancellationToken);
        await WriteEntryAsync(stream, bytes, cancellationToken);

        await WriteInt32Async(stream, updates.Count, cancellationToken);
        foreach (var update in updates)
        {
            var syncId = update.SyncId ?? "";
            await WriteStringU16Async(stream, syncId, cancellationToken);
            await WriteInt64Async(stream, update.Id, cancellationToken);
            await WriteInt64Async(stream, update.ModifiedTime, cancellationToken);
            await WriteStringI32Async(stream, update.Title, cancellationToken);
            await WriteStringI32Async(stream, update.Artist, cancellationToken);
            await WriteStringI32Async(stream, update.Lyrics, cancellationToken);
            await WriteStringI32Async(stream, update.Author, cancellationToken);
            await WriteStringI32Async(stream, update.Note, cancellationToken);
        }
    }

    private static async Task WriteEntryAsync(
        Stream stream,
        byte[]? data,
        CancellationToken cancellationToken)
    {
        data ??= [];
        await WriteInt32Async(stream, data.Length, cancellationToken);
        await stream.WriteAsync(data, cancellationToken);
    }

    private static async Task WriteStringU16Async(Stream stream, string value, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(value ?? "");
        if (bytes.Length > ushort.MaxValue)
        {
            throw new InvalidOperationException("String too long.");
        }
        await WriteUInt16Async(stream, (ushort)bytes.Length, cancellationToken);
        if (bytes.Length > 0)
        {
            await stream.WriteAsync(bytes, cancellationToken);
        }
    }

    private static async Task WriteStringI32Async(Stream stream, string? value, CancellationToken cancellationToken)
    {
        if (value == null)
        {
            await WriteInt32Async(stream, -1, cancellationToken);
            return;
        }
        var bytes = Encoding.UTF8.GetBytes(value);
        await WriteInt32Async(stream, bytes.Length, cancellationToken);
        if (bytes.Length > 0)
        {
            await stream.WriteAsync(bytes, cancellationToken);
        }
    }

    private static async Task WriteUInt16Async(Stream stream, ushort value, CancellationToken cancellationToken)
    {
        var buffer = new byte[2];
        buffer[0] = (byte)(value >> 8);
        buffer[1] = (byte)(value & 0xFF);
        await stream.WriteAsync(buffer, cancellationToken);
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

    private static async Task WriteInt64Async(Stream stream, long value, CancellationToken cancellationToken)
    {
        var buffer = new byte[8];
        buffer[0] = (byte)((value >> 56) & 0xFF);
        buffer[1] = (byte)((value >> 48) & 0xFF);
        buffer[2] = (byte)((value >> 40) & 0xFF);
        buffer[3] = (byte)((value >> 32) & 0xFF);
        buffer[4] = (byte)((value >> 24) & 0xFF);
        buffer[5] = (byte)((value >> 16) & 0xFF);
        buffer[6] = (byte)((value >> 8) & 0xFF);
        buffer[7] = (byte)(value & 0xFF);
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

    private sealed class HolyricsApplyResultJson
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("custom_md5")]
        public string? CustomMd5 { get; set; }

        [JsonPropertyName("bytes_base64")]
        public string? BytesBase64 { get; set; }
    }
}

public sealed record HolyricsApplyResult(string Name, string CustomMd5, byte[] Bytes);
