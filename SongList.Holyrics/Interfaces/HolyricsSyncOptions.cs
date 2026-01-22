namespace SongList.Holyrics.Interfaces;

public class HolyricsSyncOptions
{
    public string DriveFolder { get; init; } = "sync_holyrics_files";
    public string TokenBase64 { get; init; } = string.Empty;
    public string ProjectName { get; init; } = "sync";
    public string[] AuthRefreshUrls =
    [
        "https://www.holyrics.com.br/api/googledrive/authRefresh.php",
        "https://cgdzfezcbpp24soebe3w4ubps40gysrz.lambda-url.us-east-2.on.aws/authRefresh"
    ];
}