using System.ComponentModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SongList.Web.Controllers;
using SongList.Web.Services;
using AppContext = SongList.Web.AppContext;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.
builder.Services.AddDbContext<AppContext>(o => o.UseNpgsql(config["DbConnectionString"]));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication()
    .AddScheme<TokenAuthOptions, TokenAuthHandler>("Token", null);
builder.Services.Configure<TokenAuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.AddSwaggerGen(c => c.CustomSchemaIds(x => x.GetCustomAttributes<DisplayNameAttribute>().SingleOrDefault()?.DisplayName ?? x.Name));
builder.Services.AddCors(x => x.AddDefaultPolicy(x =>
    x.SetIsOriginAllowed(_ => true)
        .AllowCredentials()
        .AllowAnyHeader()
));
builder.Services.AddResponseCompression();
builder.Services.AddSignalR();


builder.Services.AddScoped<HistoryService>();
builder.Services.AddSingleton<OpenedSongsManager>();
builder.Services.AddHostedService(s => s.GetRequiredService<OpenedSongsManager>());


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