using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SongList.Web.Controllers;

namespace SongList.Web.Services;

public class OpenedSongsManager(IHubContext<SongsHub> hubContext): BackgroundService
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _updates = new();
    private readonly ConcurrentDictionary<string, int> _clientSongs = new();
    private readonly ConcurrentDictionary<int, int> _openedSongs = new();

    public async Task OpenSong(string connectionId, int songId)
    {
        _updates[connectionId] = DateTimeOffset.Now;
        if (_clientSongs.TryGetValue(connectionId, out var currentSongId))
        {
            if (currentSongId != songId)
            {
                await CloseSong(connectionId, currentSongId);
            }
        }

        _clientSongs[connectionId] = songId;
        await AddCounter(songId, 1);
    }

    public async Task CloseSong(string connectionId, int songId)
    {
        _updates[connectionId] = DateTimeOffset.Now;
        if (_clientSongs.TryGetValue(connectionId, out var currentSongId) && currentSongId == songId)
        {
            _clientSongs.TryRemove(connectionId, out _);
            await AddCounter(songId, -1);
        }
    }

    public async Task CloseClientSongs(string connectionId)
    {
        _updates.TryRemove(connectionId, out _);
        if (_clientSongs.TryRemove(connectionId, out var songId))
        {
            await AddCounter(songId, -1);
        }
    }

    private async Task AddCounter(int songId, int value)
    {
        _openedSongs.TryAdd(songId, 0);
        var counterValue = Math.Max(0, _openedSongs[songId] + value);
        _openedSongs[songId] = counterValue;
        var valueForClient = Math.Max(0, counterValue - 1);
        await hubContext.Clients.All.SendAsync("updateCounter", songId, valueForClient);
    }

    public Dictionary<int, int> GetOpenSongs()
    {
        return _openedSongs.Where(pair => pair.Value > 0)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            var limitTime = DateTimeOffset.Now.AddMinutes(-10);
            var outdated = _updates
                .Where(x => x.Value < limitTime)
                .Select(x => x.Key)
                .ToList();
            foreach (var connectionId in outdated)
            {
                await CloseClientSongs(connectionId);
            }
        }
    }
}