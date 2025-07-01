using System.Net.Http.Headers;
using HolyricsCompanion.History;
using HolyricsCompanion.Holyrics;
using HolyricsCompanion.Slides;
using HolyricsCompanion.Songlist;
using HolyricsCompanion.Workers;
using LiteDB;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(s => s.ServiceName = "Songlist Holyrics companion");
builder.Services.AddHttpClient();
builder.Services.AddSingleton<LiteDatabase>(_ => new LiteDatabase("db.litedb"));
builder.Services.AddSingleton<HistoryRepository>();
builder.Services.AddSingleton<SongSlideRepository>();
builder.Services.AddSingleton<VerseRepository>();
builder.Services.AddScoped<HolyricsClient>();


builder.Services.AddScoped<SonglistClient>();
builder.Services.AddHttpClient<SonglistClient>((sp, cl) =>
{
    var options = sp.GetRequiredService<IOptionsMonitor<SonglistSettings>>().CurrentValue;
    cl.BaseAddress = new Uri(options.BaseUrl);
    cl.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(options.Token);
});

builder.Services.AddHostedService<HolyricsWorker>();
builder.Services.AddHostedService<HistoryOutboxWorker>();
builder.Services.AddHostedService<SlidesWorker>();
builder.Services.AddHostedService<SlidesOutboxWorker>();

builder.Services.Configure<WorkersSettings>(builder.Configuration.GetSection("Workers"));
builder.Services.Configure<SonglistSettings>(builder.Configuration.GetSection("SonglistSettings"));
builder.Services.Configure<HolyricsSettings>(builder.Configuration.GetSection("HolyricsSettings"));

var host = builder.Build();

host.Run();
