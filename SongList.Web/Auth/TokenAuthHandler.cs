using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace SongList.Web.Auth;

public class TokenAuthOptions : AuthenticationSchemeOptions
{
    public string Token { get; init; }
}

public class TokenAuthHandler(IOptionsMonitor<TokenAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<TokenAuthOptions>(options, logger, encoder)
{
    private readonly IOptionsMonitor<TokenAuthOptions> _options = options;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var configuredToken = _options.CurrentValue.Token;
        if (string.IsNullOrEmpty(configuredToken))
        {
            return Task.FromResult(AuthenticateResult.Fail("Token auth is not configured"));
        }

        if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var requestToken))
        {
            return Task.FromResult(AuthenticateResult.Fail("missing token"));
        }

        if (!FixedTimeEquals(requestToken.ToString(), configuredToken))
        {
            return Task.FromResult(AuthenticateResult.Fail("invalid token"));
        }

        var claimsPrincipal = new ClaimsPrincipal();
        var claimsIdentity = new ClaimsIdentity("JWT");
        claimsPrincipal.AddIdentity(claimsIdentity);
        var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    // Постоянновременное сравнение shared-secret: время ответа не зависит от длины совпадающего префикса.
    private static bool FixedTimeEquals(string? a, string? b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
        {
            return false;
        }

        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return ba.Length == bb.Length && CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}