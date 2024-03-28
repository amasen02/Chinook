using Chinook.ClientModels;

namespace Chinook.Services.Interfaces
{
    public interface IPlaylistService
    {
        ClientModels.Playlist GetPlaylist(long playlistId, string userId);
        Models.Playlist GetFavoritePlaylist(string userId);
    }
}
