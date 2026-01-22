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
    public async Task<string> TgAdmin([FromBody] Dictionary<string, object> fields)
    {
        return await tgAuthService.GetToken(fields.ToDictionary(x => x.Key, x => x.Value.ToString()));
    }

    [HttpGet("user", Name = "getUser")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public string GetUser()
    {
        return User.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
    }
}