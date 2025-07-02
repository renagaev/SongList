using System.ComponentModel;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Minio;
using SongList.Web.Auth;
using SongList.Web.Controllers;
using SongList.Web.Services;
using Telegram.Bot;
using Telegram.Bot.Extensions.LoginWidget;
using AppContext = SongList.Web.AppContext;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.
builder.Services.AddDbContext<AppContext>(o => o.UseNpgsql(config["DbConnectionString"]));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOptions<JwtOptions>().BindConfiguration("Jwt");
builder.Services.AddAuthentication()
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
builder.Services.Configure<TokenAuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.AddSwaggerGen(c =>
    c.CustomSchemaIds(x => x.GetCustomAttributes<DisplayNameAttribute>().SingleOrDefault()?.DisplayName ?? x.Name));
builder.Services.AddCors(x => x.AddDefaultPolicy(x =>
    x.SetIsOriginAllowed(_ => true)
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod()
));
builder.Services.AddResponseCompression();
builder.Services.AddSignalR();


builder.Services.AddScoped<SongHistoryService>();
builder.Services.AddScoped<VerseHistoryService>();
builder.Services.AddSingleton<OpenedSongsManager>();
builder.Services.AddScoped<SongService>();

builder.Services.AddOptions<S3Settings>().BindConfiguration(nameof(S3Settings));
builder.Services.AddSingleton<IMinioClient>(s =>
{
    var options = s.GetRequiredService<IOptions<S3Settings>>().Value;
    var factory = new MinioClientFactory(client => client
        .WithRegion(options.Region)
        .WithEndpoint(options.BaseUrl)
        .WithSSL()
        .WithCredentials(options.AccessKey, options.SecretKey));
    return factory.CreateClient();
});
builder.Services.AddScoped<AttachmentsService>();


builder.Services.AddHostedService(s => s.GetRequiredService<OpenedSongsManager>());

builder.Services.AddOptions<TgOptions>().BindConfiguration("Tg");
builder.Services.AddSingleton<LoginWidget>(s =>
{
    var options = s.GetRequiredService<IOptions<TgOptions>>().Value;
    return new LoginWidget(options.Token);
});
builder.Services.AddSingleton<ITelegramBotClient>(s =>
{
    var options = s.GetRequiredService<IOptions<TgOptions>>().Value;
    return new TelegramBotClient(options.Token);
});
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

app.UseResponseCompression();
app.UseFileServer();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<SongsHub>("/songsHub");
app.MapControllers();


app.Run();