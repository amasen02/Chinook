using Chinook.ClientModels;
using Chinook.Models;
using Chinook.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
 
namespace Chinook.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IDbContextFactory<ChinookContext> _dbFactory;
        private ChinookContext _dbContext;

        public ArtistService(IDbContextFactory<ChinookContext> dbFactory)
        {
            _dbFactory = dbFactory;
            _dbContext = _dbFactory.CreateDbContext();
        }

        public Artist GetArtist(long artistId)
        {
            var artist = _dbContext.Artists.Find(artistId);
            if (artist == null)
            {
                throw new Exception($"Artist with ID {artistId} not found.");
            }
            return artist;
        }

        public List<ClientModels.PlaylistTrack> GetTracks(long artistId, string userId)
        {
            var tracks = _dbContext.Tracks
                .Where(t => t.Album.ArtistId == artistId)
                .Select(t => new ClientModels.PlaylistTrack
                {
                    AlbumTitle = t.Album.Title,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Any(pt => pt.UserPlaylists.Any(up => up.UserId == userId))
                })
                .ToList();
            return tracks;
        }

        public void FavoriteTrack(long trackId, string userId)
        {
            // Synchronous implementation for favoriting a track
        }

        public void UnfavoriteTrack(long trackId, string userId)
        {
            // Synchronous implementation for unfavoriting a track
        }

        public void AddTrackToPlaylist(long trackId, string playlistName, string userId)
        {
            // Synchronous implementation for adding a track to a playlist
        }
    }
}

