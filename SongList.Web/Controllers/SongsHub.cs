using Microsoft.AspNetCore.SignalR;
using SongList.Web.Services;

namespace SongList.Web.Controllers;

public class SongsHub(OpenedSongsManager manager): Hub
{
    public Task OpenSong(int songId)
    {
        return manager.OpenSong(Context.ConnectionId, songId);
    }

    public Task CloseSong(int songId)
    {
        return manager.CloseSong(Context.ConnectionId, songId);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return manager.CloseClientSongs(Context.ConnectionId);
    }
}