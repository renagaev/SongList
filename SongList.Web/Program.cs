using System.ComponentModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using SongList.Web.Controllers;
using SongList.Web.Extensions;
using AppContext = SongList.Web.AppContext;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.
builder.Services.AddDbContext<AppContext>(o => o.UseNpgsql(config["DbConnectionString"]));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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


builder.Services.AddAuth(builder.Configuration);
builder.Services.AddApplicationServices();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppContext>();
    await context.Database.MigrateAsync();
}

app.UseResponseCompression();
app.UseFileServer(new FileServerOptions
{
    StaticFileOptions =
    {
        OnPrepareResponse = ctx =>
        {
            var path = ctx.Context.Request.Path.Value ?? "";
            var headers = ctx.Context.Response.GetTypedHeaders();
            headers.CacheControl = path.StartsWith("/assets/", StringComparison.OrdinalIgnoreCase)
                ? new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromDays(365), Extensions = { new NameValueHeaderValue("immutable") } }
                : new CacheControlHeaderValue { NoCache = true, MustRevalidate = true };
        }
    }
});
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