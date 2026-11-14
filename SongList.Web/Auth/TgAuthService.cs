using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Telegram.Bot;
using Telegram.Bot.Extensions.LoginWidget;

namespace SongList.Web.Auth;

public class TgAuthService(LoginWidget loginWidget, ITelegramBotClient bot, IOptions<TgOptions> options, IOptions<JwtOptions> jwtOptions)
{
    public async Task<string> GetToken(Dictionary<string, string> fields)
    {
        var auth = loginWidget.CheckAuthorization(fields);
        if (auth != Authorization.Valid)
        {
            throw new UnauthorizedAccessException("Invalid auth");
        }

        if (!fields.TryGetValue("id", out var idValue) || !long.TryParse(idValue, out var id))
        {
            throw new UnauthorizedAccessException("Invalid auth");
        }

        var member = await bot.GetChatMember(options.Value.ChatId, id);
        if (!member.IsInChat)
        {
            throw new UnauthorizedAccessException("User is not in admin group");
        }
        var userName = $"{member.User.FirstName} {member.User.LastName}".Trim();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, userName),
        };

        var token = new JwtSecurityToken(jwtOptions.Value.Issuer,
            jwtOptions.Value.Issuer,
            claims,
            expires: DateTime.UtcNow.Add(jwtOptions.Value.Expiration),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}