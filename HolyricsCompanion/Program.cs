using HolyricsCompanion.Holyrics;
using HolyricsCompanion.Songlist;
using HolyricsCompanion.Storage;
using HolyricsCompanion.Workers;
using LiteDB;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(s => s.ServiceName = "Songlist Holyrics companion");
builder.Services.AddHttpClient();
builder.Services.AddSingleton<LiteDatabase>(_ => new LiteDatabase("db.litedb"));
builder.Services.AddSingleton<Repository>();
builder.Services.AddScoped<HolyricsClient>();
builder.Services.AddScoped<SonglistClient>();
builder.Services.AddHostedService<HolyricsWorker>();
builder.Services.AddHostedService<OutboxWorker>();
builder.Services.Configure<WorkersSettings>(builder.Configuration.GetSection("Workers"));
builder.Services.Configure<SonglistSettings>(builder.Configuration.GetSection("SonglistSettings"));
builder.Services.Configure<HolyricsSettings>(builder.Configuration.GetSection("HolyricsSettings"));

var host = builder.Build();

host.Run();
