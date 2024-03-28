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
            var artist = _dbContext.Artists
                        .Include(a => a.Albums) // Assuming you might want to include related entities like Albums
                        .FirstOrDefault(a => a.ArtistId == artistId);

            if (artist == null)
            {
                throw new Exception($"Artist with ID {artistId} not found.");
            }
            return artist;
        }

        public List<PlaylistTrack> GetTracks(long artistId, string userId)
        {
            var tracks = _dbContext.Tracks
                        .Where(t => t.Album.ArtistId == artistId)
                        .Select(t => new PlaylistTrack
                        {
                            AlbumTitle = t.Album.Title,
                            TrackId = t.TrackId,
                            TrackName = t.Name,
                            IsFavorite = t.Playlists.Any(pt => pt.UserPlaylists.Any(up => up.UserId == userId))
                        })
                        .ToList();
            return tracks;
        }

        public async Task<List<Artist>> GetArtists()
        {
            return _dbContext.Artists.ToList();
        }

        public async Task<List<Artist>> GetArtistsByName(string name)
        {
            return await _dbContext.Artists
                            .Where(a => a.Name.ToLower().Contains(name.ToLower()))
                            .ToListAsync();
        }
        public async Task<List<Album>> GetAlbumsForArtist(int artistId)
        {
            return _dbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
        }

    }
}

