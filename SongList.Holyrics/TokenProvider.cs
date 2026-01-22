using System.Text.Json;
using Microsoft.Extensions.Options;
using SongList.Holyrics.Interfaces;

namespace SongList.Holyrics;

internal class TokenProvider(IOptions<HolyricsSyncOptions> options, HttpClient httpClient, IHolyricsTokenStorage tokenStorage)
{
    private HolyricsAuthToken? _token;

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        _token ??= await tokenStorage.GetTokenAsync(cancellationToken);

        if (_token.IsExpired())
        {
            await RefreshAsync(cancellationToken);
            await tokenStorage.SaveTokenAsync(_token, cancellationToken);
        }

        return _token.AccessToken;
    }

    private static readonly string[] AuthRefreshUrls =
    [
        "https://www.holyrics.com.br/api/googledrive/authRefresh.php",
        "https://cgdzfezcbpp24soebe3w4ubps40gysrz.lambda-url.us-east-2.on.aws/authRefresh"
    ];

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var form = new Dictionary<string, string>
        {
            ["refresh_token"] = _token.RefreshToken,
            ["project_name"] = options.Value.ProjectName
        };

        var response = await PostFormWithFallbackAsync(AuthRefreshUrls, form, cancellationToken);
        var updated = DeserializeToken(response);
        _token.AccessToken = updated.AccessToken;
        _token.CreatedAt = updated.CreatedAt;
        _token.ExpiresIn = updated.ExpiresIn;
    }

    private async Task<string> PostFormWithFallbackAsync(
        IEnumerable<string> urls,
        IDictionary<string, string> form,
        CancellationToken cancellationToken)
    {
        Exception? last = null;
        foreach (var url in urls)
        {
            try
            {
                using var content = new FormUrlEncodedContent(form);
                var response = await httpClient.PostAsync(url, content, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode || string.Equals(body.Trim(), "invalid_request", StringComparison.Ordinal))
                {
                    last = new InvalidOperationException("invalid_request");
                    continue;
                }

                return body;
            }
            catch (Exception ex)
            {
                last = ex;
            }
        }

        throw last ?? new InvalidOperationException("Refresh request failed.");
    }

    private static HolyricsAuthToken DeserializeToken(string json)
    {
        var token = JsonSerializer.Deserialize<HolyricsAuthToken>(json);
        if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new InvalidOperationException("Invalid token received.");
        }

        return token;
    }
}
