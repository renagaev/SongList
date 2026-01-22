using System.Text;
using Microsoft.IdentityModel.Tokens;
using SongList.Web.Auth;

namespace SongList.Web.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<JwtOptions>().BindConfiguration("Jwt");
        services.AddAuthentication()
            .AddScheme<TokenAuthOptions, TokenAuthHandler>("Token", null)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
                };
            });
        services.AddOptions<TokenAuthOptions>().BindConfiguration("Auth");
        return services;
    }
}