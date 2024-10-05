using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace SongList.Web.Controllers;

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
        Request.Headers.TryGetValue(HeaderNames.Authorization, out var requestToken);

        if (requestToken == _options.CurrentValue.Token)
        {
            var claimsPrincipal = new ClaimsPrincipal();
            var claimsIdentity = new ClaimsIdentity("JWT");
            claimsPrincipal.AddIdentity(claimsIdentity);
            var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.Fail("invalid token"));
    }
}