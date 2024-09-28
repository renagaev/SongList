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
builder.Services.AddSwaggerGen();
builder.Services.AddCors(x => x.AddDefaultPolicy(x =>
    x.SetIsOriginAllowed(_ => true)
        .AllowCredentials()
        .AllowAnyHeader()
));
builder.Services.AddResponseCompression();
builder.Services.AddSignalR();
builder.Services.AddSingleton<OpenedSongsManager>();
builder.Services.AddHostedService(s => s.GetService<OpenedSongsManager>());

var app = builder.Build();
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppContext>().Database.MigrateAsync();
}

app.UseResponseCompression();
app.UseFileServer();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();

app.MapHub<SongsHub>("/songsHub");
app.MapControllers();


app.Run();