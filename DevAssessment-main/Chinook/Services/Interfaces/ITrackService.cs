using Chinook.Models;

namespace Chinook.Services.Interfaces
{
    public interface ITrackService
    {
        Chinook.ClientModels.PlaylistTrack GetTrackByID(string userId, long trackId);
        List<Chinook.ClientModels.PlaylistTrack> GetTracks(long artistId, string userId);
        void FavoriteTrack(long trackId, string userId);
        void UnfavoriteTrack(long trackId, string userId);
        void AddTrackToPlaylist(long trackId, string playlistName, string userId);
        void RemoveTrack(long trackId, long playlistId);
    }
}
