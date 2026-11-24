using System.ComponentModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using SongList.Web.Controllers;
using SongList.Web.Extensions;
using AppContext = SongList.Web.AppContext;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

ValidateRequiredSecrets(config);

// Add services to the container.
builder.Services.AddDbContext<AppContext>(o => o.UseNpgsql(config["DbConnectionString"]));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddProblemDetails();

builder.Services.AddSwaggerGen(c =>
    c.CustomSchemaIds(x => x.GetCustomAttributes<DisplayNameAttribute>().SingleOrDefault()?.DisplayName ?? x.Name));

var corsOrigins = new List<string>(config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>());
if (builder.Environment.IsDevelopment())
{
    corsOrigins.Add("http://localhost:5173");
}

builder.Services.AddCors(o => o.AddPolicy("DefaultCors", p =>
    p.WithOrigins(corsOrigins.ToArray())
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddResponseCompression();
builder.Services.AddSignalR();


builder.Services.AddAuth(builder.Configuration);
builder.Services.AddApplicationServices();


var app = builder.Build();

app.UseExceptionHandler();

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

app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<SongsHub>("/songsHub");
app.MapControllers();


app.Run();


static void ValidateRequiredSecrets(IConfiguration config)
{
    var required = new (string Key, string Description)[]
    {
        ("DbConnectionString", "строка подключения к БД (DbConnectionString)"),
        ("Jwt:Key", "ключ подписи JWT (Jwt__Key)")
    };

    foreach (var (key, description) in required)
    {
        var value = config[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Конфигурация {key} ({description}) не задана или содержит значение по умолчанию. " +
                "Задайте её через переменную окружения (см. .env.example / user-secrets).");
        }
    }
}