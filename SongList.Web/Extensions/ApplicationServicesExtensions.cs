using Microsoft.Extensions.Options;
using Minio;
using SongList.Holyrics;
using SongList.Holyrics.Interfaces;
using SongList.ServicePredict;
using SongList.Web.Auth;
using SongList.Web.Services;
using SongList.Web.UseCases.SyncHolyricsSongs;
using Telegram.Bot;
using Telegram.Bot.Extensions.LoginWidget;

namespace SongList.Web.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<SongHistoryService>();
        services.AddScoped<VerseHistoryService>();
        services.AddSingleton<OpenedSongsManager>();
        services.AddScoped<SongService>();
        services.AddScoped<SongUpdateNotifier>();
        services.AddSingleton<IServicePredictor, OnnxServicePredictor>();
        
        services.AddScoped<SyncHolyricsSongsHandler>();


        services.AddHostedService(s => s.GetRequiredService<OpenedSongsManager>());


        services.AddHolyricsSync();

        services.AddAttachmentsServices();
        services.AddTelegramServices();
        
        
        return services;
    }

    public static IServiceCollection AddHolyricsSync(this IServiceCollection services)
    {
        services.AddSingleton<IHolyricsTokenStorage, HolyricsTokenStorage>();
        services.AddHolyricsSyncClient();
        services.AddHostedService<HolyricsSyncHostedService>();
        
        
        return services;
    }

    public static IServiceCollection AddAttachmentsServices(this IServiceCollection services)
    {
        services.AddOptions<S3Settings>().BindConfiguration(nameof(S3Settings));
        services.AddSingleton<IMinioClient>(s =>
        {
            var options = s.GetRequiredService<IOptions<S3Settings>>().Value;
            var factory = new MinioClientFactory(client => client
                .WithRegion(options.Region)
                .WithEndpoint(options.BaseUrl)
                .WithSSL()
                .WithCredentials(options.AccessKey, options.SecretKey));
            return factory.CreateClient();
        });
        services.AddScoped<AttachmentsService>();

        return services;
    }

    public static IServiceCollection AddTelegramServices(this IServiceCollection services)
    {
        services.AddOptions<TgOptions>().BindConfiguration("Tg");
        services.AddSingleton<LoginWidget>(s =>
        {
            var options = s.GetRequiredService<IOptions<TgOptions>>().Value;
            return new LoginWidget(options.Token);
        });
        services.AddSingleton<ITelegramBotClient>(s =>
        {
            var options = s.GetRequiredService<IOptions<TgOptions>>().Value;
            return new TelegramBotClient(options.Token);
        });
        services.AddScoped<TgAuthService>();

        return services;
    }
}