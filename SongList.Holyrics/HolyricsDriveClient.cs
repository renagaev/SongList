using System.Net.Http.Headers;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Download;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Upload;
using File = Google.Apis.Drive.v3.Data.File;

namespace SongList.Holyrics;

internal sealed class HolyricsDriveClient(TokenProvider tokenProvider)
{
    public const string DefaultFolder = "sync_holyrics_files";

    private readonly DriveService _drive = new(new BaseClientService.Initializer
    {
        ApplicationName = "SongList.Holyrics",
        HttpClientInitializer = new BearerTokenInitializer(tokenProvider.GetTokenAsync),
    });

    public async Task<IReadOnlyList<HolyricsDriveFile>> ListFilesAsync(
        string folder,
        CancellationToken cancellationToken)
    {
        var all = new List<HolyricsDriveFile>();
        string? pageToken = null;

        do
        {
            var page = await ListFilesPageAsync(folder, pageToken, cancellationToken);

            if (page.Files != null)
            {
                all.AddRange(page.Files.Select(Map));
            }

            pageToken = page.NextPageToken;
        }
        while (!string.IsNullOrWhiteSpace(pageToken));

        return all;
    }

    public async Task<byte[]> DownloadFileAsync(string fileId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileId))
            throw new ArgumentException("FileId is empty", nameof(fileId));

        // Get(fileId) + DownloadAsync = скачивание контента (alt=media)
        // Работает для обычных файлов в Drive. Для Google Docs/Sheets нужно Export.
        var request = _drive.Files.Get(fileId);

        await using var ms = new MemoryStream();
        var progress = await request.DownloadAsync(ms, cancellationToken);

        if (progress.Status != DownloadStatus.Completed)
        {
            throw new InvalidOperationException(
                $"Drive download failed. Status={progress.Status}, Exception={progress.Exception?.Message}");
        }

        return ms.ToArray();
    }

    public async Task UpdateFileAsync(
        HolyricsDriveFile file,
        byte[] content,
        string customMd5,
        CancellationToken cancellationToken)
    {
        var appProps = new Dictionary<string, string>(file.AppProperties)
        {
            ["custom_md5"] = customMd5
        };

        // Важно: это метаданные, которые мы хотим пропатчить
        var metadata = new File
        {
            AppProperties = appProps
        };

        await using var stream = new MemoryStream(content);

        // Update(fileMetadata, fileId, contentStream, contentType)
        // Это "media upload": обновит и метаданные, и содержимое
        var upload = _drive.Files.Update(metadata, file.Id, stream, "application/octet-stream");

        // Можно попросить вернуть минимум полей (не обязательно)
        upload.Fields = "id,appProperties,modifiedTime,md5Checksum";

        var result = await upload.UploadAsync(cancellationToken);

        if (result.Status != UploadStatus.Completed)
        {
            throw new InvalidOperationException(
                $"Drive upload failed. Status={result.Status}, Exception={result.Exception?.Message}");
        }
    }

    private async Task<FileList> ListFilesPageAsync(
        string folder,
        string? pageToken,
        CancellationToken cancellationToken)
    {
        // Поиск именно в appDataFolder
        // + фильтр по appProperties.folder == folder
        var q = $"appProperties has {{ key='folder' and value='{EscapeForDriveQuery(folder)}' }}";

        var request = _drive.Files.List();
        request.Spaces = "appDataFolder";
        request.PageSize = 1000;
        request.Fields = "nextPageToken,files(id,name,mimeType,modifiedTime,modifiedByMeTime,size,md5Checksum,appProperties)";
        request.Q = q;

        if (!string.IsNullOrWhiteSpace(pageToken))
            request.PageToken = pageToken;

        return await request.ExecuteAsync(cancellationToken);
    }

    private static HolyricsDriveFile Map(File f)
        => new()
        {
            Id = f.Id,
            Name = f.Name,
            MimeType = f.MimeType,
            ModifiedAt = f.ModifiedTimeDateTimeOffset,
            Size = f.Size,
            AppProperties = f.AppProperties
        };

    /// <summary>
    /// В Drive query строки в одинарных кавычках, поэтому минимум — экранировать одиночную кавычку.
    /// Drive использует backslash для escape внутри query.
    /// </summary>
    private static string EscapeForDriveQuery(string value)
        => value.Replace("'", "\\'");

    /// <summary>
    /// Подкладываем Bearer токен на каждый запрос Google.Apis через interceptor.
    /// </summary>
    private sealed class BearerTokenInitializer(Func<CancellationToken, Task<string>> tokenProvider)
        : IConfigurableHttpClientInitializer, IHttpExecuteInterceptor
    {
        private readonly Func<CancellationToken, Task<string>> _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));

        public void Initialize(ConfigurableHttpClient httpClient) => httpClient.MessageHandler.AddExecuteInterceptor(this);

        public async Task InterceptAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenProvider(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
