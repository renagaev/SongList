using Microsoft.Extensions.DependencyInjection;
using SongList.Holyrics.Interfaces;
using SongList.Holyrics.JavaHelper;

namespace SongList.Holyrics;

public static class DependencyInjection
{
    public static IServiceCollection AddHolyricsSyncClient(this IServiceCollection services)
    {
        services.AddSingleton<TokenProvider>();
        services.AddSingleton<MusicDecoder>();
        services.AddHttpClient<TokenProvider>();
        services.AddScoped<HolyricsDriveClient>();
        
        
        services.AddScoped<IHolyricsSyncClient, HolyricsSyncClient>();
        return services;
    }
}
