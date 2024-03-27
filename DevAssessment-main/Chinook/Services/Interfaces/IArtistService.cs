using Chinook.Models;

namespace Chinook.Services.Interfaces
{
    public interface IArtistService
    {
        Artist GetArtist(long artistId);
        List<Chinook.ClientModels.PlaylistTrack> GetTracks(long artistId, string userId);
        void FavoriteTrack(long trackId, string userId);
        void UnfavoriteTrack(long trackId, string userId);
        void AddTrackToPlaylist(long trackId, string playlistName, string userId);
    }
}
