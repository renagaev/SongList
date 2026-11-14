using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SongList.Web.Auth;

namespace SongList.Web.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(TgAuthService tgAuthService) : ControllerBase
{
    [HttpPost("tg-admin", Name = "getTgAdminToken")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> TgAdmin([FromBody] Dictionary<string, object?> fields)
    {
        try
        {
            return await tgAuthService.GetToken(
                fields.ToDictionary(x => x.Key, x => x.Value?.ToString() ?? string.Empty));
        }
        catch (Exception e) when (e is UnauthorizedAccessException or Telegram.Bot.Exceptions.ApiRequestException)
        {
            // ApiRequestException: Telegram не нашёл пользователя/чат (например, админ не писал боту).
            return Unauthorized();
        }
    }

    [HttpGet("user", Name = "getUser")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public string GetUser()
    {
        return User.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
    }
}