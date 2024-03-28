using Chinook.Models;

namespace Chinook.Services.Interfaces
{
    public interface IArtistService
    {
        Task<List<Artist>> GetArtists();
        Task<List<Artist>> GetArtistsByName(string name);
        Task<List<Album>> GetAlbumsForArtist(int artistId);
        Artist GetArtist(long artistId);
        List<Chinook.ClientModels.PlaylistTrack> GetTracks(long artistId, string userId);
    }
}
